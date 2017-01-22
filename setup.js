var fs = require('fs')
var inquirer = require('inquirer')

var questions = [
    {
        type: 'input',
        name: 'testusername',
        message: 'Test username: '
    },
    {
        type: 'input',
        name: 'testhost',
        message: 'Test hostname: '
    },
    {
        type: 'input',
        name: 'sshkeypath',
        message: 'Path to your ssh private key: '
    },
    {
        type: 'input',
        name: 'devbottoken',
        message: 'Local development bot token: '
    },
    {
        type: 'input',
        name: 'testbottoken',
        message: 'Test bot token: '
    },
    {
        type: 'input',
        name: 'prodbottoken',
        message: 'Prod bot token: '
    }
]

inquirer.prompt(questions).then(function (answers) {
    answers.sshkeypath = answers.sshkeypath.replace(/['"]+/g, '')

    answersJson = JSON.stringify(answers, null, 4)

    console.log(answersJson)
    fs.writeFileSync('secret.json', JSON.stringify(answers, null, 4))
    console.log('Secrets have been saved to secret.json')

    cb()
})
