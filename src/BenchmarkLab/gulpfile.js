/// <binding Clean='clean' AfterBuild='default' />
"use strict";

var gulp = require("gulp"),
    concat = require("gulp-concat"),
    cssmin = require("gulp-cssmin"),
    uglify = require("gulp-uglify");
const rimraf = require("rimraf");
var ts = require("gulp-typescript");
var tsProject = ts.createProject("Scripts/tsconfig.json");

var webroot = "./wwwroot/";
var scriptsRoot = "./Scripts/";

var paths = {
    js: webroot + "js/**/*.js",
    scripts: [scriptsRoot + "**/*.js", scriptsRoot + "**/*.map"],
    minJs: webroot + "js/**/*.min.js",
    css: webroot + "css/**/*.css",
    minCss: webroot + "css/**/*.min.css",
    concatJsDest: webroot + "js/site.min.js",
    concatCssDest: webroot + "css/site.min.css",
    partials: scriptsRoot + "partials/*.html"
};

async function cleanJs() {
    const del = (await import('del'));
    rimraf.sync(paths.concatJsDest);
    // TODO: this path does not exist. need to delete benchmarklab.js and testrunner.js
    return del.deleteSync(['wwwroot/scripts/**/*']);
}

async function cleanCss() {
    const del = (await import('del'));
    rimraf.sync(paths.concatCssDest);
    return del.deleteSync([]);
}

gulp.task("clean:js", cleanJs);
gulp.task("clean:css", cleanCss);
gulp.task("clean", gulp.parallel("clean:js", "clean:css"));

gulp.task("min:js",
    async function(done) {
        return gulp.src([paths.js, "!" + paths.minJs], { base: "." })
            .pipe(concat(paths.concatJsDest))
            .pipe(uglify())
            .pipe(gulp.dest("."))
            .on('end', done);
    });

gulp.task("min:css",
    async function (done) {
        return gulp.src([paths.css, "!" + paths.minCss])
            .pipe(concat(paths.concatCssDest))
            .pipe(cssmin())
            .pipe(gulp.dest("."))
            .on('end', done);
    });

gulp.task("build:ts",
    async function () {
        gulp.src(paths.scripts).pipe(gulp.dest('wwwroot/js'));
        return tsProject.src().pipe(tsProject()).js.pipe(gulp.dest('wwwroot/js'));
    });

gulp.task("min", gulp.parallel("min:js", "min:css"));
gulp.task("default", gulp.series("clean", "build:ts", "min"));
