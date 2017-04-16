const exec = require('child_process').exec
const copydir = require('copy-dir');
const fs = require('fs')
const fstream = require('fstream')
const gulp = require('gulp')
const shell = require('gulp-shell')
const gutil = require('gulp-util');
const shelljs = require('shelljs')
const tar = require('tar')
const waitUntil = require('wait-until')
const zlib = require('zlib')

const secretFilePath = './secret.json'

const projectName = 'BundtBot'
const projectFolder = `src/${projectName}`
const projectFileName = `project.json`
const projectFilePath = `${projectFolder}/${projectFileName}`

const buildOutputFolder = `${projectFolder}/bin/debug/netcoreapp1.1`
const publishFolder = `${buildOutputFolder}/publish`

const tarFileName = `${projectName}.tar.gz`
const viewsFolderName = `Views`
const viewsFolder = `${projectFolder}/${viewsFolderName}`

const testFolder = 'test'
const rateLimitTestsProjectName = 'RateLimitTests'
const rateLimitTestsProjectFolder = `${testFolder}/${rateLimitTestsProjectName}`
const rateLimitTestsOutputFolder = `${rateLimitTestsProjectFolder}/bin/Debug/netcoreapp1.1`

const libopus64linux = 'bin/opus/linux-1.1.2-x86-64/libopus.so.0.5.2'
const libopus64windows = 'bin/opus/windows-x86-64/libopus.dll'
const libsodium64linux = 'bin/libsodium/linux-1.0.12-x86-64/libsodium.so.18.2.0'
const libsodium64windows = 'bin/libsodium/windows-1.0.12-x86-64/libsodium.dll'

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
        .pipe(fstream.Writer({ 'path': tarFileName }))
}