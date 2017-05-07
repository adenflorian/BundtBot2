const exec = require('child_process').exec
const spawn = require('child_process').spawn;
const copydir = require('copy-dir')
const fs = require('fs')
const gulp = require('gulp')
const rimraf = require('rimraf')
const client = require('scp2')
const node_ssh = require('node-ssh')
const ssh2Client = require('ssh2').Client;

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
	cleanTarTask(() => {
		rimraf(projectBinFolder, (err) => {
			if (err) throw err
			rimraf(projectBinFolder, (err) => {
				if (err) throw err
				cb()
			})
		})
	})
})

gulp.task('dotnet-restore', myShellTask(`dotnet restore ${projectFilePath}`))

gulp.task('dotnet-build', ['dotnet-restore'], myShellTask(`dotnet build ${projectFilePath}`))

gulp.task('build', ['dotnet-build'], () => {
	copySync('config/dev/config.json', buildOutputFolder + '/config.json')
	copyTokenSync(secret.devbottoken, buildOutputFolder)
	copywindowsbinsbuildSync()
})

gulp.task('run', ['build'], myShellTask(`dotnet BundtBot.dll`, { cwd: buildOutputFolder }))

gulp.task('publish', ['dotnet-restore'], (cb) => {
	myShell(`dotnet publish ${projectFilePath}`, null, () => {
		copylinuxbinspublishSync()
		copyTokenSync(secret.testbottoken, publishFolder)
		console.log('Copying config to publish folder...')
		copySync('config/test/config.json', publishFolder + '/config.json')
		cb()
	})
})

gulp.task('deploy', ['publish'], (cb) => {
	myShell('node do tar', null, () => {
		UploadToTestServerUsingSftpTask(() => {
			sshDeploy(() => {
				cleanTarTask(cb)
			})
		})
	})
})

gulp.task('ssh-deploy', sshDeploy)

function sshDeploy(cb) {
	var destinationFolder = "bundtbot"
	const ssh = new node_ssh()

	ssh.connect({
		host: secret.testhost,
		username: secret.testusername,
		privateKey: secret.sshkeypath
	}).then(() => {
		doSshCommands(ssh, [
			"pwd",
			"echo 'stopping bundtbot service'",
			"service bundtbot stop",
			"echo 'deleting old app'",
			`rm -rf ${destinationFolder}`,
			`mkdir ${destinationFolder}`,
			"echo 'unpacking new app'",
			`tar xzf ${tarFileName} -C ${destinationFolder}`,
			`chmod +x ${destinationFolder}/${projectName}.dll`,
			`chmod +x ${destinationFolder}/youtube-dl.exe`,
			"echo 'starting bundtbot service'",
			"service bundtbot start"
		], () => {
			ssh.dispose()
			cb()
		})
	})
}

function doSshCommands(ssh, commands, cb) {
	if (commands.length === 0) {
		cb()
	} else {
		ssh.execCommand(commands.shift()).then((result) => {
			if (result.stdout.length > 0) console.log('STDOUT: ' + result.stdout)
			if (result.stderr.length > 0) console.log('STDERR: ' + result.stderr)
			doSshCommands(ssh, commands, cb)
		})
	}
}

// ***Start test commands***

gulp.task('test', myShellTask('dotnet test test/BundtBotTests/BundtBotTests.csproj',
	{ verbose: true }))

gulp.task('rate-limiter-tests', myShellTask(`dotnet test ${rateLimitTestsProjectFolder}/${rateLimitTestsProjectName}.csproj`))

// ***TesterBot***

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

gulp.task('run-testerbot', ['build-testerbot'], myShellTask(`dotnet ${testerBotProjectName}.dll`, { cwd: testerBotOutputFolder }))

// ***Start remote server commands***

gulp.task('rlogs', (cb) => {
	var conn = new ssh2Client();
	conn.on('ready', () => {
		console.log('Client :: ready');
		conn.exec('journalctl -f -o cat -u bundtbot.service', (err, stream) => {
			if (err) throw err;
			stream.on('close', (code, signal) => {
				console.log('Stream :: close :: code: ' + code + ', signal: ' + signal);
				conn.end();
				cb()
			}).on('data', (data) => {
				console.log(data.toString().trim());
			}).stderr.on('data', (data) => {
				console.log(data.toString().trim());
			});
		});
	}).connect({
		host: secret.testhost,
		port: 22,
		username: secret.testusername,
		privateKey: fs.readFileSync(secret.sshkeypath)
	});
})

gulp.task('setup-server', myShellTask('grunt sshexec:setup'))

gulp.task('restart-remote', myShellTask('grunt sshexec:restart'))

// ***functions***

function copyTokenSync(token, outputFolder) {
	console.log('Copying token...')
	fs.writeFileSync(`${outputFolder}/bottoken`, token)
}

function copywindowsbinsbuildSync() {
	console.log('Copying windows binaries to output folder...')
	copySync('bin/opus/windows-1.1.2-x86-64/opus.dll', buildOutputFolder + '/libopus.dll')
	copySync('bin/libsodium/windows-1.0.12-x86-64/libsodium.dll', buildOutputFolder + '/libsodium.dll')
	copySync('bin/youtube-dl/windows/youtube-dl.exe', buildOutputFolder + '/youtube-dl.exe')
	copySync('bin/ffmpeg/windows/ffmpeg.exe', buildOutputFolder + '/ffmpeg.exe')
	copySync('bin/ffmpeg/windows/ffprobe.exe', buildOutputFolder + '/ffprobe.exe')
}

function copylinuxbinspublishSync() {
	console.log('Copying linux binaries to publish folder...')
	copySync('bin/opus/linux-1.1.2-x86-64/libopus.so.0.5.2', publishFolder + '/libopus.dll')
	copySync('bin/libsodium/linux-1.0.12-x86-64/libsodium.so.18.2.0', publishFolder + '/libsodium.dll')
	copySync('bin/youtube-dl/linux/youtube-dl.exe', publishFolder + '/youtube-dl.exe')
}

function UploadToTestServerUsingSftpTask(cb) {
	console.log(`Uploading ${tarFileName} to ${secret.testhost}...`)

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

function cleanTarTask(cb) {
	fs.unlink(`${projectName}.tar.gz`, (err) => {
		if (err) console.log(err)
		if (cb) cb()
	})
}

function myShell(command, options, cb) {
	myShellTask(command, options)(cb)
}

function myShellTask(command, options) {
	return (cb) => {
		var split = command.split(' ')
		var cmd = split[0]
		var args = []

		for (var i = 1; i < split.length; i++) {
			args[i - 1] = split[i]
		}
		console.log(`Running command '${cmd}' with args '${args}'`)

		var commandSpawn

		if (options) {
			commandSpawn = spawn(cmd, args, options)
		} else {
			commandSpawn = spawn(cmd, args)
		}

		commandSpawn.stdout.on('data', (data) => {
			console.log(`${data.toString().trim()}`)
		})

		commandSpawn.stderr.on('data', (data) => {
			console.log(`stderr: ${data.toString().trim()}`)
		})

		commandSpawn.on('error', (err) => {
			console.error('on error: ' + err.toString().trim())
		})

		commandSpawn.on('close', (code) => {
			console.log(`child process exited with code ${code}`)
			cb()
		})
	}
}

function copySync(srcFile, destFile) {
	fs.createReadStream(srcFile).pipe(fs.createWriteStream(destFile))
}
