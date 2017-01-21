module.exports = function (grunt) {

    var secret = grunt.file.readJSON('secret.json');
    var bundtbotfile = "bundtbot.tar"
    var destinationFolder = "bundtbot"
    var executable = "BundtBot.dll"

    // Project configuration.
    grunt.initConfig({
        pkg: grunt.file.readJSON('package.json'),
        sftp: {
            deploy: {
                files: {
                    "./": bundtbotfile
                },
                options: {
                    host: secret.host,
                    username: secret.username,
                    privateKey: grunt.file.read(secret.sshkeypath),
                    showProgress: true
                }
            }
        },
        sshexec: {
            deploy: {
                command: [
                    "pwd",
                    "echo 'stopping bundtbot service'",
                    "service bundtbot stop",
                    "echo 'deleting old app'",
                    `rm -rf ${destinationFolder}`,
                    `mkdir ${destinationFolder}`,
                    "echo 'unpacking new app'",
                    `tar xf ${bundtbotfile} -C ${destinationFolder}`,
                    `chmod +x ${destinationFolder}/${executable}`,
                    "echo 'starting bundtbot service'",
                    "service bundtbot start"
                ],
                options: {
                    host: secret.host,
                    username: secret.username,
                    privateKey: grunt.file.read(secret.sshkeypath)
                }
            }
        }
    });

    grunt.loadNpmTasks('grunt-ssh');

};