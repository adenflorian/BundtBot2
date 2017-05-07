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

function copyToken(token, outputFolder){
	fs.writeFileSync(`${outputFolder}/bottoken`, token)
}

gulp.task('copyconfigdev', ['dotnet-build'], () => {
	copy('config/dev/config.json', buildOutputFolder + '/config.json')
})

gulp.task('copyconfigtest', ['publish'], () => {
	copy('config/test/config.json', publishFolder + '/config.json')
})

gulp.task('build', ['dotnet-build', 'copyconfigdev'], () => {
	copyToken(secret.devbottoken, buildOutputFolder)
	copywindowsbinsbuild()
})

gulp.task('run', ['build'], myShell(`dotnet BundtBot.dll`, { cwd: buildOutputFolder}))

gulp.task('publish', ['dotnet-restore'], (cb) => {
	myShell(`dotnet publish ${projectFilePath}`, () => {
		copylinuxbinspublish()
		copyToken(secret.testbottoken, publishFolder)
		cb()
	})
})

gulp.task('tar', ['publish', 'copyconfigtest'], (cb) => {
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

gulp.task('run-testerbot', ['build-testerbot'], myShell(`dotnet ${testerBotProjectName}.dll`, { cwd: testerBotOutputFolder }))

// Start remote server commands

// TODO Make use something native to node, cross platform
gulp.task('rlogs', myShell(`ssh ${secret.testusername}@${secret.testhost} "journalctl -f -o cat -u bundtbot.service"`))

gulp.task('setup-server', myShell('grunt sshexec:setup'))

gulp.task('restart-remote', myShell('grunt sshexec:restart'))

function copywindowsbinsbuild() {
	copy('bin/opus/windows-1.1.2-x86-64/opus.dll', buildOutputFolder + '/libopus.dll')
	copy('bin/libsodium/windows-1.0.12-x86-64/libsodium.dll', buildOutputFolder + '/libsodium.dll')
	copy('bin/youtube-dl/windows/youtube-dl.exe', buildOutputFolder + '/youtube-dl.exe')
	copy('bin/ffmpeg/windows/ffmpeg.exe', buildOutputFolder + '/ffmpeg.exe')
	copy('bin/ffmpeg/windows/ffprobe.exe', buildOutputFolder + '/ffprobe.exe')
}

function copylinuxbinspublish() {
	copy('bin/opus/linux-1.1.2-x86-64/libopus.so.0.5.2', publishFolder + '/libopus.dll')
	copy('bin/libsodium/linux-1.0.12-x86-64/libsodium.so.18.2.0', publishFolder + '/libsodium.dll')
	copy('bin/youtube-dl/linux/youtube-dl.exe', publishFolder + '/youtube-dl.exe')
}

function sftpDeploy(cb) {
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

function cleanTar(cb) {
	fs.unlink(`${projectName}.tar.gz`, (err) => {
		if (err) console.log(err)
		if (cb) cb()
	})
}

function myShell(command, options) {
	var split = command.split(' ')
	var cmd = split[0]
	var args = []

	for (var i = 1; i < split.length; i++) {
		args[i - 1] = split[i]
	}

	return (cb) => {
		var commandSpawn
		if (options) {
			commandSpawn = spawn(cmd, args, options)
		} else {
			commandSpawn = spawn(cmd, args)
		}

		commandSpawn.stdout.on('data', (data) => {
			console.log(`stdout: ${data}`)
		})

		commandSpawn.stderr.on('data', (data) => {
			console.log(`stderr: ${data}`)
		})

		commandSpawn.on('error', (err) => {
			console.error('on error: ' + err)
		})

		commandSpawn.on('close', (code) => {
			console.log(`child process exited with code ${code}`)
			cb()
		})
	}
}

function copy(srcFile, destFile) {
	fs.createReadStream(srcFile).pipe(fs.createWriteStream(destFile))
}
