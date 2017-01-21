var fs = require('fs')
var gulp = require('gulp')
var shell = require('gulp-shell')
var tar = require('tar-fs')
var waitUntil = require('wait-until')

var secret;
var bundtbotTar = "bundtbot.tar"

if (fs.existsSync('./secret.json')) {
	secret = JSON.parse(fs.readFileSync('./secret.json'))
	require('gulp-grunt')(gulp); // add all the gruntfile tasks to gulp
} else {
	gulp.stop("***Run 'node setup.js' before using gulp!***")
}

gulp.task('clean', function () {
	fs.unlink(bundtbotTar)
})

gulp.task('build', shell.task('dotnet publish src/BundtBot/BundtBot.csproj', { verbose: true }))

gulp.task('tar', ['build'], function (cb) {
	var pack = tar.pack('./src/BundtBot/bin/debug/netcoreapp1.0/publish/')
		.pipe(fs.createWriteStream(bundtbotTar))
	waitUntil()
		.interval(1000)
		.times(50)
		.condition(function () {
			console.log('bytes written: ' + pack.bytesWritten)
			return pack._writableState.ended
		})
		.done(function (result) {
			cb()
		})
})

gulp.task('sftpdeploy', ['tar'], shell.task('grunt sftp:deploy', { verbose: true, }))

gulp.task('sshdeploy', ['sftpdeploy'], shell.task('grunt sshexec:deploy', { verbose: true, }))

gulp.task('deploy', ['build', 'tar', 'sftpdeploy', 'sshdeploy'])

gulp.task('run', ['build'], shell.task([
	'dotnet src/BundtBot/bin/debug/netcoreapp1.0/publish/BundtBot.dll'
	],
	{ verbose: true }))

gulp.task('rlogs', shell.task(
	`ssh ${secret.username}@${secret.host} "journalctl -fu bundtbot.service;"`,
	{ verbose: true }))

gulp.task('setup-server', shell.task(
	`bash scripts/setup_server.sh ${secret.username} ${secret.host}`,
	{ verbose: true }))
