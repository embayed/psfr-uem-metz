const gulp = require("gulp");
const concat = require("gulp-concat");
const uglify = require("gulp-uglify");
const esbuild = require("esbuild");

/* -------------------------------
   1) Bundle CustomPages (ESM)
-------------------------------- */
function bundleCustomPages() {
  return esbuild.build({
    entryPoints: ["src/js/CustomPages.js"],
    bundle: true,
    minify: false,
    sourcemap: false,
    format: "iife",
    target: ["es2018"],
    outfile: "dist/_custompages.bundle.js"
  });
}

/* ----------------------------------------
   2) Concat ONLY PSFR_CtAccessRules
----------------------------------------- */
function concatCtAccessRules() {
  return gulp
    .src(
      [
        // Explicit order = no surprises
        "src/PSFR_CtAccessRules/js/Constants.js",
        "src/PSFR_CtAccessRules/js/Helpers.js",
        "src/PSFR_CtAccessRules/js/Ajax.js",
        "src/PSFR_CtAccessRules/js/Translation.js",
        "src/PSFR_CtAccessRules/js/Formio.js",
        "src/PSFR_CtAccessRules/js/State.js",
        "src/PSFR_CtAccessRules/js/Api.js",
        "src/PSFR_CtAccessRules/js/Html.js",
        "src/PSFR_CtAccessRules/js/Conditions.js",
        "src/PSFR_CtAccessRules/js/RulesTable.js",
        "src/PSFR_CtAccessRules/js/Targets.js",
        "src/PSFR_CtAccessRules/js/Page.js",
        "src/PSFR_CtAccessRules/js/Router.js",
        "src/PSFR_CtAccessRules/js/Bootstrap.js"
      ],
      { allowEmpty: true }
    )
    .pipe(concat("_ctaccessrules.concat.js"))
    .pipe(gulp.dest("dist"));
}

/* ----------------------------------------
   3) Concat legacy JS + CtAccessRules
----------------------------------------- */
function concatLegacy() {
  return gulp
    .src(
      [
        "dist/_ctaccessrules.concat.js",
        "src/PSFR_threads/uem-thread.js",
        "src/js/**/*.js",
        "!src/js/CustomPages.js"
      ],
      { allowEmpty: true }
    )
    .pipe(concat("_legacy.concat.js"))
    .pipe(gulp.dest("dist"));
}

/* ----------------------------------------
   4) Final build
----------------------------------------- */
function buildFinal() {
  return gulp
    .src(
      [
        "dist/_legacy.concat.js",
        "dist/_custompages.bundle.js"
      ],
      { allowEmpty: true }
    )
    .pipe(concat("Custom.js"))
    .pipe(
      uglify({
        compress: {
          drop_debugger: false,
          drop_console: false
        },
        mangle: false
      })
    )
    .pipe(gulp.dest("dist"));
}

/* ----------------------------------------
   Tasks
----------------------------------------- */
const scripts = gulp.series(
  bundleCustomPages,
  concatCtAccessRules,
  concatLegacy,
  buildFinal
);

exports.scripts = scripts;
exports.default = scripts;
