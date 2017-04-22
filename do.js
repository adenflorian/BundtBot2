const fstream = require('fstream')
const tar = require('tar')
const zlib = require('zlib')

const projectName = 'BundtBot'
const projectFolder = `src/${projectName}`
const buildOutputFolder = `${projectFolder}/bin/debug/netcoreapp1.1`
const publishFolder = `${buildOutputFolder}/publish`

var secret;

switch (process.argv[2])
{
    case 'tar':
        tar2()
        break
    default: 
        console.error("command not found")
        break
}

function tar2()
{
    fstream.Reader({ 'path': publishFolder, 'type': 'Directory' }) /* Read the source directory */
        .pipe(tar.Pack({ 'fromBase': true })) /* Convert the directory to a .tar file */
        .pipe(zlib.Gzip()) /* Compress the .tar file */
        .pipe(fstream.Writer({ 'path': `${projectName}.tar.gz` }))
}