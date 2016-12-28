var gulp = require('gulp')
var shell = require('gulp-shell')

gulp.task('default', function() {
 	// place code for your default task here
})

gulp.task('build', shell.task([
	'cd src\\BundtBot && dotnet publish -f netcoreapp1.0 -c release && cd ..\\..'
	],
	{verbose: true}))

gulp.task('deploy', ['build'], shell.task(
	'scripts\\deploy.sh',
	{verbose: true}))

gulp.task('runlocal', ['build'], shell.task(
	'cd src\\BundtBot && dotnet run && cd ..\\..',
	{verbose: true}))

gulp.task('watch', function() {
	var watcher = gulp.watch('**/*.cs', ['default'])
	watcher.on('change', function(event) {
	  console.log('File ' + event.path + ' was ' + event.type + ', running tasks...')
	})
})

