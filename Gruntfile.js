module.exports = function (grunt) {

    var secret = grunt.file.readJSON('secret.json');
    var sshOptions = {
        host: secret.testhost,
        username: secret.testusername,
        privateKey: grunt.file.read(secret.sshkeypath)
    }

    // Project configuration.
    grunt.initConfig({
        pkg: grunt.file.readJSON('package.json'),
        sshexec: {
            restart: {
                command: [
                    "pwd",
                    "echo 'restarting bundtbot service'",
                    "service bundtbot restart"
                ],
                options: sshOptions
            },
            setup: {
                command: [
                    'apt-get update',
                    'apt-get upgrade -y',
                    'echo "Storage=persistent" >> /etc/systemd/journald.conf',
                    'echo "SystemMaxUse=100M" >> /etc/systemd/journald.conf',
                    'echo "ForwardToSyslog=no" >> /etc/systemd/journald.conf',
                    'apt-get install -y dotnet-dev-1.0.1',
                    'apt-get install -y nginx',
                    'apt-get install -y python',
                    'apt-get install -y libav-tools',
                    'curl -o /etc/nginx/sites-available/bundtbot https://raw.githubusercontent.com/AdenFlorian/BundtBotBeta/master/nginx/sites/bundtbot',
                    'service nginx start'
                ],
                options: sshOptions
            }
        }
    });

    grunt.loadNpmTasks('grunt-ssh');
};