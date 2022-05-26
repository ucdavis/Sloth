const path = require("path");

const MiniCssExtractPlugin = require("mini-css-extract-plugin");
const OptimizeCssAssetsPlugin = require("optimize-css-assets-webpack-plugin");
const TerserPlugin = require("terser-webpack-plugin");

const bundleOutputDir = "./wwwroot/dist";

module.exports = (env) => {
  const isDevBuild = !(env && env.prod);
  console.log("dev build?", isDevBuild);
  return [
    {
      stats: { modules: false },
      entry: {
        site: "./wwwroot/js/site.js",
      },
      resolve: {
        extensions: [".js", ".jsx"],
      },
      output: {
        path: path.join(__dirname, bundleOutputDir),
        filename: "site.js",
        publicPath: "dist/",
      },
      devServer: {
        devMiddleware: {
          publicPath: "/dev",
        },
      },
      mode: isDevBuild ? "development" : "production",
      module: {
        rules: [
          {
            test: /\.scss$/,
            use: [
              MiniCssExtractPlugin.loader,
              "css-loader",
              "postcss-loader",
              "sass-loader",
            ],
          },
          { test: /\.(png|jpg|jpeg|gif|svg)$/, use: "url-loader?limit=25000" },
        ],
      },
      optimization: {
        minimizer: isDevBuild
          ? []
          : [
              new TerserPlugin({
                parallel: true,
                terserOptions: {
                  sourceMap: true,
                },
              }),
            ],
      },
      plugins: [
        new MiniCssExtractPlugin({
          // Options similar to the same options in webpackOptions.output
          // all options are optional
          filename: isDevBuild ? "[name].css" : "[name].min.css",
          chunkFilename: "[id].css",
          ignoreOrder: false, // Enable to remove warnings about conflicting order
        }),
      ],
    },
  ];
};

