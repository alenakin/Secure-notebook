const webpack = require("webpack");
const path = require("path");

module.exports = {
    entry: "./src/index.jsx",
    output:{
        path: path.resolve(__dirname, "./public"),
        publicPath: "public/",
        filename: "bundle.js"
    },
    watch: true,
    devtool: "source-map",
    module: {
        rules: [
            {
                enforce: "pre",
                test: /\.jsx?$/,
                include: path.join(__dirname, "src"),
                exclude: [/node_modules/, /public/],
                loader: "eslint-loader",
                options: {
                    formatter: require("eslint-friendly-formatter"),
                    emitWarning: true,
                    emitError: true,
                    failOnWarning: true,
                    failOnError: true
                }
            },
            {
                test: /\.js$/,
                loader: "babel-loader",
                exclude: [/node_modules/, /public/]
            },
            {
                test: /\.jsx?$/,
                exclude: /(node_modules)/,
                loader: "babel-loader",
                exclude: [/node_modules/, /public/],
                options:{
                    presets:["env", "react", "es2015", "stage-2"]
                }
            },
            {
                test: /\.less$/,
                use: [{
                    loader: "style-loader" // creates style nodes from JS strings
                }, {
                    loader: "css-loader" // translates CSS into CommonJS
                }, {
                    loader: "less-loader" // compiles Less to CSS
                }]
            },
            {
                test: /\.(jpe?g|png|gif|svg)$/i,
                loaders: ["file-loader?context=src/images&name=images/[path][name].[ext]", {
                loader: "image-webpack-loader",
                query: {
                    mozjpeg: {
                    progressive: true,
                    },
                    gifsicle: {
                    interlaced: false,
                    },
                    optipng: {
                    optimizationLevel: 4,
                    },
                    pngquant: {
                    quality: "75-90",
                    speed: 3,
                    },
                },
                }],
                exclude: /node_modules/,
                include: __dirname,
            },
            {
                test: /\.(eot|ttf|woff|woff2)$/,
                loader: "file-loader?name=fonts/[name].[ext]"
            }
        ]
    },
    plugins: [
        new webpack.ProvidePlugin({
            _: "lodash"
        })
    ]
};