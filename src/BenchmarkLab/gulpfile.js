/// <binding Clean='clean' AfterBuild='default' />
"use strict";

var gulp = require("gulp"),
    rimraf = require("rimraf"),
    concat = require("gulp-concat"),
    cssmin = require("gulp-cssmin"),
    uglify = require("gulp-uglify"),
    del = require("del");
var ts = require("gulp-typescript");
var tsProject = ts.createProject("Scripts/tsconfig.json");
var browserify = require("browserify");
var source = require('vinyl-source-stream');
var tsify = require("tsify");
var watchify = require("watchify");
var gutil = require("gulp-util");

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

gulp.task("clean:js",
    function(cb) {
        rimraf(paths.concatJsDest, cb);
        del(['wwwroot/scripts/**/*']);
    });

gulp.task("clean:css",
    function(cb) {
        rimraf(paths.concatCssDest, cb);
    });

gulp.task("clean", gulp.parallel("clean:js", "clean:css"));

gulp.task("min:js",
    function() {
        return gulp.src([paths.js, "!" + paths.minJs], { base: "." })
            .pipe(concat(paths.concatJsDest))
            .pipe(uglify())
            .pipe(gulp.dest("."));
    });

gulp.task("min:css",
    function() {
        return gulp.src([paths.css, "!" + paths.minCss])
            .pipe(concat(paths.concatCssDest))
            .pipe(cssmin())
            .pipe(gulp.dest("."));
    });

gulp.task("min", gulp.parallel("min:js", "min:css"));

gulp.task("copy-html",
    function() {
        return gulp.src(paths.partials)
            .pipe(gulp.dest('wwwroot/js/partials'));
    });

gulp.task('default',
    gulp.series("copy-html"),
    function() {
        gulp.src(paths.scripts).pipe(gulp.dest('wwwroot/js'));
        tsProject.src().pipe(tsProject()).js.pipe(gulp.dest('wwwroot/js'));
    });
