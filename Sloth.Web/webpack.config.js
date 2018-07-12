const path = require('path');
const ExtractTextPlugin = require('extract-text-webpack-plugin');

const bundleOutputDir = './wwwroot/dist';

module.exports = (env) => {
    const isDevBuild = !(env && env.prod);
    return [{
        stats: { modules: false },
        entry: {
          site: './wwwroot/js/site.js',
        },
        resolve: {
            extensions: ['.js', '.jsx'],
       },
        output: {
            path: path.join(__dirname, bundleOutputDir),
            filename: 'site.js',
            publicPath: 'dist/'
        },
        module: {
            rules: [
                {
                  test: /\.scss$/,
                  use: ExtractTextPlugin.extract({
                    use: [{
                      loader: 'css-loader',
                      options: {
                        importLoaders: 1,
                        minimize: !isDevBuild,
                        sourceMap: isDevBuild,
                      }
                    }, {
                      loader: 'postcss-loader',
                      options: {
                        sourceMap: true,
                      }
                    }, {
                      loader: 'sass-loader',
                      options: {
                        sourceMap: true,
                      }
                    }]
                  })
                },
                { test: /\.(png|jpg|jpeg|gif|svg)$/, use: 'url-loader?limit=25000' }
            ]
        },
        plugins: [
            new ExtractTextPlugin({
              filename: isDevBuild ? 'site.css' : 'site.min.css',
            }),
        ]
    }];
};
