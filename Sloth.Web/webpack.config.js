const path = require('path');
const webpack = require('webpack');

const MiniCssExtractPlugin = require('mini-css-extract-plugin');
const OptimizeCssAssetsPlugin = require('optimize-css-assets-webpack-plugin');
const TerserPlugin = require('terser-webpack-plugin');
const BundleAnalyzerPlugin = require('webpack-bundle-analyzer')
    .BundleAnalyzerPlugin;


const bundleOutputDir = './wwwroot/dist';

module.exports = (env) => {
    const isDevBuild = !(env && env.prod);
    const isAnalyze = env && env.analyze;

    return [{
        stats: { modules: false },
        entry: {
          site: './wwwroot/js/site.js',
        },
        resolve: {
            extensions: ['.js', '.jsx'],
            modules: ['node_modules']
       },
        output: {
            path: path.join(__dirname, bundleOutputDir),
            filename: 'site.js',
            publicPath: 'dist/'
        },
        devServer: {
            clientLogLevel: 'info',
            compress: true,
            port: process.env.DEV_SERVER_PORT || 8080,
            injectClient: false,
            // transportMode: 'ws',  // TODO: move to WS once it's no longer experimental
            contentBase: path.resolve(__dirname, 'wwwroot')
        },
        mode: isDevBuild ? 'development' : 'production',
        module: {
            rules: [
                {
                  test: /\.scss$/,
                  include: /wwwroot/,
                  use: [
                    !isDevBuild
                    ? MiniCssExtractPlugin.loader
                    : {
                        loader: 'style-loader'
                    },
                    {
                      loader: 'css-loader',
                      options: {
                        importLoaders: 2,
                        sourceMap: isDevBuild,
                      }
                    },
                    //{
                    //  loader: 'postcss-loader',
                    //  options: {
                    //    loader: 'sass-loader',
                    //    sourceMap: true,
                    //  }
                    //},
                    {
                      loader: 'sass-loader',
                      options: {
                        sourceMap: true,
                      }
                    }
                  ]
                },
                { test: /\.(png|jpg|jpeg|gif|svg)$/, use: 'url-loader?limit=25000' }
            ]
        },
        optimization: {
          minimizer: isDevBuild
            ? []
            : [
                new TerserPlugin({
                  cache: true,
                  parallel: true,
                  sourceMap: true
                }),
                new OptimizeCssAssetsPlugin({})
              ]
        },
        plugins: [
          ...(isDevBuild
            ? [
                // Plugins that apply in development builds only
                new webpack.EvalSourceMapDevToolPlugin({
                  filename: '[file].map' // Remove this line if you prefer inline source maps
                })
              ]
            : [
                // Plugins that apply in production builds only
                new webpack.SourceMapDevToolPlugin({
                  filename: '[file].map' // Remove this line if you prefer inline source maps
                }),
                new MiniCssExtractPlugin({
                  filename: 'site.min.css'
                })
              ]),
              // Webpack Bundle Analyzer
              // https://github.com/th0r/webpack-bundle-analyzer
              ...(isAnalyze ? [new BundleAnalyzerPlugin()] : [])
        ]
    }];
};
