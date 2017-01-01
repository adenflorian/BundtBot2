var gulp = require('gulp')
var shell = require('gulp-shell')
var user = "remote username"
var host = "remote ip"

gulp.task('default', function() {
 	// place code for your default task here
})

gulp.task('build', shell.task([
	'cd src\\BundtBot && dotnet publish -f netcoreapp1.0 -c release && cd ..\\..'
	],
	{verbose: true}))

gulp.task('deploy', ['build'], shell.task(
	`bash scripts\\deploy.sh ${user} ${host}`,
	{verbose: true}))

gulp.task('run', ['build'], shell.task([
	'cd src\\BundtBot\\bin\\release\\netcoreapp1.0\\publish && dotnet BundtBot.dll && cd ..\\..\\..\\..\\..\\..'
	],
	{verbose: true}))

gulp.task('rlogs', shell.task(
	`ssh ${user}@${host} "journalctl -fu bundtbot.service;"`,
	{verbose: true}))

gulp.task('setup-server', shell.task(
	`bash scripts\\setup_server.sh ${user} ${host}`,
	{verbose: true}))

gulp.task('watch', function() {
	var watcher = gulp.watch('**/*.cs', ['default'])
	watcher.on('change', function(event) {
	  console.log('File ' + event.path + ' was ' + event.type + ', running tasks...')
	})
})

