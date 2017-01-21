var fs = require('fs')
var inquirer = require('inquirer')

var questions = [
    {
        type: 'input',
        name: 'username',
        message: 'Username: '
    },
    {
        type: 'input',
        name: 'host',
        message: 'Hostname: '
    },
    {
        type: 'input',
        name: 'sshkeypath',
        message: 'Path to your ssh private key: '
    },
]

inquirer.prompt(questions).then(function (answers) {
    answers.sshkeypath = answers.sshkeypath.replace(/['"]+/g, '')
    answersJson = JSON.stringify(answers, null, 4)
    console.log(answersJson)
    fs.writeFileSync('secret.json', JSON.stringify(answers, null, 4))
    console.log('Secrets have been saved to secret.json')
    cb()
})
