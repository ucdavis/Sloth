const path = require('path');
const webpack = require('webpack');
const ExtractTextPlugin = require('extract-text-webpack-plugin');
const CheckerPlugin = require('awesome-typescript-loader').CheckerPlugin;
const CopyWebpackPlugin = require('copy-webpack-plugin');
const bundleOutputDir = './wwwroot/dist';

assets = [
  'jquery/dist/jquery.js',
  'bootstrap/dist/js/bootstrap.js',
  'popper.js/dist/umd/popper.js',
  'datatables.net/js/jquery.datatables.js',
  'datatables.net-bs/js/datatables.bootstrap.js',
  'datatables.net-bs/css/datatables.bootstrap.css',
];

module.exports = (env) => {
    const isDevBuild = !(env && env.prod);
    return [{
        stats: { modules: false },
        entry: {
            'boot': './ClientApp/boot.tsx',
            'react': ['event-source-polyfill', 'isomorphic-fetch', 'react', 'react-dom', 'react-router-dom'],
            'runtime': [],
            'site': './wwwroot/js/site.js'
        },
        resolve: {
            extensions: ['.js', '.jsx', '.ts', '.tsx'],
       },
        output: {
            path: path.join(__dirname, bundleOutputDir),
            filename: '[name].js',
            publicPath: 'dist/'
        },
        module: {
            rules: [
                { test: /\.tsx?$/, include: /ClientApp/, use: 'awesome-typescript-loader?silent=true' },
                {
                  test: /\.css$/,
                  use: ExtractTextPlugin.extract({
                    fallback: 'style-loader',
                    use: [{
                      loader: 'css-loader',
                      options: {
                        minimize: !isDevBuild
                      }
                    }]
                  })
                },
                {
                  test: /\.scss$/,
                  use: ExtractTextPlugin.extract({
                    fallback: 'style-loader',
                    use: [{
                      loader: 'css-loader',
                      options: {
                        minimize: !isDevBuild
                      }
                    }, 'postcss-loader', 'sass-loader']
                  })
                },
                { test: /\.(png|jpg|jpeg|gif|svg)$/, use: 'url-loader?limit=25000' }
            ]
        },
        plugins: [
            new CheckerPlugin(),
            new ExtractTextPlugin({
              disable: isDevBuild,
              filename: 'site.css'
            }),
            new webpack.optimize.CommonsChunkPlugin({
                name: ['react', 'runtime'],
                minChunks: Infinity
            }),
            new CopyWebpackPlugin(
              assets.map(a => {
                return {
                  from: path.resolve(__dirname, `./node_modules/${a}`),
                  to: path.resolve(__dirname, './wwwroot/lib')
                };
              })
            )
        ].concat(isDevBuild ? [
            // Plugins that apply in development builds only
            new webpack.SourceMapDevToolPlugin({
                filename: '[file].map', // Remove this line if you prefer inline source maps
                moduleFilenameTemplate: path.relative(bundleOutputDir, '[resourcePath]') // Point sourcemap entries to the original file locations on disk
            })
        ] : [
            // Plugins that apply in production builds only
            new webpack.optimize.UglifyJsPlugin(),
        ])
    }];
};
