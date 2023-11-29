const path = require('path');

module.exports = {
    entry: './wwwroot/assets/js/grpc/GameSessionManager.js',
    output: {
        filename: 'GameSessionManager.js',
        path: path.resolve(__dirname, 'wwwroot/dist'),
        library: {
            name: 'grpc',
            type: 'umd',
        },
    },
};
