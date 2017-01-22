module.exports = function (grunt) {

    var secret = grunt.file.readJSON('secret.json');
    var bundtbotfile = "bundtbot.tar"
    var destinationFolder = "bundtbot"
    var executable = "BundtBot.dll"
    var sshOptions = {
        host: secret.testhost,
        username: secret.testusername,
        privateKey: grunt.file.read(secret.sshkeypath)
    }

    // Project configuration.
    grunt.initConfig({
        pkg: grunt.file.readJSON('package.json'),
        sftp: {
            deploy: {
                files: {
                    "./": bundtbotfile
                },
                options: {
                    host: secret.testhost,
                    username: secret.testusername,
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
                options: sshOptions
            },
            setup: {
                command: [
                    "apt-get update; apt-get upgrade -y",
                    'echo "deb [arch=amd64] https://apt-mo.trafficmanager.net/repos/dotnet-release/ xenial main" > /etc/apt/sources.list.d/dotnetdev.list',
                    'apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 417A0893',
                    'apt-get update',
                    'apt-get install -y dotnet-dev-1.0.0-preview2.1-003177',
                    'apt-get install -y nginx',
                    'curl -o /etc/nginx/sites-available/bundtbot https://raw.githubusercontent.com/AdenFlorian/BundtBotBeta/master/nginx/sites/bundtbot',
                    'service nginx start'
                ],
                options: sshOptions
            }
        }
    });

    grunt.loadNpmTasks('grunt-ssh');

};