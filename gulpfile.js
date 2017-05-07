const exec = require('child_process').exec
const spawn = require('child_process').spawn;
const copydir = require('copy-dir')
const fs = require('fs')
const gulp = require('gulp')
const rimraf = require('rimraf')
const client = require('scp2')

const secretFilePath = './secret.json'

const projectName = 'BundtBot'
const projectFolder = `src/${projectName}`
const projectFileName = `${projectName}.csproj`
const projectFilePath = `${projectFolder}/${projectFileName}`

const projectBinFolder = `${projectFolder}/bin`
const buildOutputFolder = `${projectBinFolder}/debug/netcoreapp1.1`
const publishFolder = `${buildOutputFolder}/publish`

const tarFileName = `${projectName}.tar.gz`

const testFolder = 'test'
const rateLimitTestsProjectName = 'RateLimitTests'
const rateLimitTestsProjectFolder = `${testFolder}/${rateLimitTestsProjectName}`
const rateLimitTestsOutputFolder = `${rateLimitTestsProjectFolder}/bin/Debug/netcoreapp1.1`

var secret

if (fs.existsSync(secretFilePath)) {
	secret = JSON.parse(fs.readFileSync(secretFilePath))
	// Add all the gruntfile tasks to gulp
	require('gulp-grunt')(gulp)
} else {
	gulp.stop("***Run 'node setup.js' before using gulp!***")
}

gulp.task('clean', ['clean-testerbot'], (cb) => {
	cleanTar(() => {
		rimraf(projectBinFolder, (err) => {
			if (err) throw err
			rimraf(projectBinFolder, (err) => {
				if (err) throw err
				cb()
			})
		})
	})
})

gulp.task('dotnet-restore', myShell(`dotnet restore ${projectFilePath}`))

gulp.task('dotnet-build', ['dotnet-restore'], myShell(`dotnet build ${projectFilePath}`))

gulp.task('copytokendev', ['dotnet-build'], () => {
	copyToken(secret.devbottoken, buildOutputFolder)
})

gulp.task('copytokentest', ['publish'], () => {
	copyToken(secret.testbottoken, publishFolder)
})

function copyToken(token, outputFolder)
{
	fs.writeFileSync(`${outputFolder}/bottoken`, token)
}

gulp.task('copyconfigdev', ['dotnet-build'], () => {
	fs.createReadStream('config/dev/config.json').pipe(fs.createWriteStream(buildOutputFolder + '/config.json'))
})

gulp.task('copyconfigtest', ['publish'], () => {
	fs.createReadStream('config/test/config.json').pipe(fs.createWriteStream(publishFolder + '/config.json'))
})

gulp.task('build', ['dotnet-build', 'copytokendev', 'copyconfigdev'], () => {
	copywindowsbinsbuild()
})

gulp.task('run', ['build'], myShell(`dotnet BundtBot.dll`, { verbose: true, cwd: buildOutputFolder }))

function copywindowsbinsbuild() {
	fs.createReadStream('bin/opus/windows-1.1.2-x86-64/opus.dll').pipe(fs.createWriteStream(buildOutputFolder + '/libopus.dll'))
	fs.createReadStream('bin/libsodium/windows-1.0.12-x86-64/libsodium.dll').pipe(fs.createWriteStream(buildOutputFolder + '/libsodium.dll'))
	fs.createReadStream('bin/youtube-dl/windows/youtube-dl.exe').pipe(fs.createWriteStream(buildOutputFolder + '/youtube-dl.exe'))
	fs.createReadStream('bin/ffmpeg/windows/ffmpeg.exe').pipe(fs.createWriteStream(buildOutputFolder + '/ffmpeg.exe'))
	fs.createReadStream('bin/ffmpeg/windows/ffprobe.exe').pipe(fs.createWriteStream(buildOutputFolder + '/ffprobe.exe'))
}

function copylinuxbinspublish() {
	fs.createReadStream('bin/opus/linux-1.1.2-x86-64/libopus.so.0.5.2').pipe(fs.createWriteStream(publishFolder + '/libopus.dll'))
	fs.createReadStream('bin/libsodium/linux-1.0.12-x86-64/libsodium.so.18.2.0').pipe(fs.createWriteStream(publishFolder + '/libsodium.dll'))
	fs.createReadStream('bin/youtube-dl/linux/youtube-dl.exe').pipe(fs.createWriteStream(publishFolder + '/youtube-dl.exe'))
}

gulp.task('publish', ['dotnet-restore'], (cb) => {
	exec(`dotnet publish ${projectFilePath}`, (error, stdout, stderr) => {
		console.log(stdout)
		copylinuxbinspublish()
		cb()
	})
})

gulp.task('tar', ['publish', 'copytokentest', 'copyconfigtest'], (cb) => {
	exec('node do tar', (error, stdout, stderr) => {
		console.log(stdout)
		cb()
	})
})

gulp.task('sftpdeploy', ['tar'], sftpDeploy)

gulp.task('sshdeploy', ['sftpdeploy'], myShell('grunt sshexec:deploy'))

gulp.task('deploy', ['publish', 'tar', 'sftpdeploy', 'sshdeploy'], cleanTar)

// Start test commands

gulp.task('test', myShell('dotnet test test/BundtBotTests/BundtBotTests.csproj',
	{ verbose: true }))

gulp.task('rate-limiter-tests', myShell(`dotnet test ${rateLimitTestsProjectFolder}/${rateLimitTestsProjectName}.csproj`))

// TesterBot
const testerBotProjectName = 'TesterBot'
const testerBotProjectFolder = 'test/' + testerBotProjectName
const testerBotProjectFile = testerBotProjectFolder + '/' + testerBotProjectName + '.csproj'
const testerBotBinFolder = testerBotProjectFolder + '/bin'
const testerBotObjFolder = testerBotProjectFolder + '/obj'
const testerBotOutputFolder = testerBotBinFolder + '/debug/netcoreapp1.1'

gulp.task('build-testerbot', (cb) => {
	exec(`dotnet build ${testerBotProjectFile}`, (error, stdout, stderr) => {
		console.log(stdout)
		if (error) throw error
		fs.writeFileSync(testerBotOutputFolder + '/bottoken', secret.testerbottoken)
		cb()
	})
})

gulp.task('clean-testerbot', (cb) => {
	rimraf(testerBotBinFolder, (err) => {
		if (err) console.error(err)
		cb()
	})
})

gulp.task('run-testerbot', ['build-testerbot'], myShell(`dotnet ${testerBotProjectName}.dll`, { verbose: true, cwd: testerBotOutputFolder }))

// Start remote server commands

gulp.task('rlogs', myShell(
	`ssh ${secret.testusername}@${secret.testhost} "journalctl -f -o cat -u bundtbot.service"`,
	{ verbose: true }))

gulp.task('setup-server', myShell('grunt sshexec:setup'))

gulp.task('restart-remote', myShell('grunt sshexec:restart'))

function sftpDeploy(cb)
{
	client.defaults({
		port: 22,
		host: secret.testhost,
		username: secret.testusername,
		privateKey: fs.readFileSync(secret.sshkeypath)
	})

	client.on('transfer', (buffer, uploaded, total) => {
		if (uploaded % 25 == 0) {
			console.log(uploaded + '/' + total)
		}
	})

	client.upload(tarFileName, tarFileName, () => {
		client.close()
		cb()
	})
}

function cleanTar(cb)
{
	fs.unlink(`${projectName}.tar.gz`, (err) => {
		if (err) console.log(err)
		if (cb) cb()
	})
}

function myShell(command) {
	var split = command.split(' ')
	var cmd = split[0]
	var args = []

	for (var i = 1; i < split.length; i++) {
		args[i - 1] = split[i]
	}

	return (cb) => {
		const commandSpawn = spawn(cmd, args);

		commandSpawn.stdout.on('data', (data) => {
			console.log(`stdout: ${data}`);
		});

		commandSpawn.stderr.on('data', (data) => {
			console.log(`stderr: ${data}`);
		});

		commandSpawn.on('error', (err) => {
			console.error('on error: ' + err);
		});

		commandSpawn.on('close', (code) => {
			console.log(`child process exited with code ${code}`);
			cb()
		});
	}
}
