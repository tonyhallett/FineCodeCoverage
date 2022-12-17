module.exports = {
    parser : "@typescript-eslint/parser",
    parserOptions: {
      tsconfigRootDir: __dirname,
      project: "./tsconfig.json"
    },
    env: {
      commonjs : true,
      browser : true
    },
    plugins: [
     "react",
     "react-hooks",
     "@typescript-eslint"
    ],
    extends: [
      "eslint:recommended",
      "plugin:react/recommended",
      "plugin:react/jsx-runtime",
      "plugin:react-hooks/recommended",
      "plugin:@typescript-eslint/recommended",
      "plugin:@typescript-eslint/recommended-requiring-type-checking",
      "prettier"
    ],
    settings:{
      react:{
        "version": "detect"
      }
    },
    root: true
}