const ChatService = (io) => {
    // Store active admins
    const admins = new Set();

    io.on('connection', (socket) => {
        const { userId, username, role } = socket.handshake.query;

        console.log(`[Socket] New connection: ${username} (${role})`);

        // Join room based on User ID
        if (userId) {
            socket.join(`user_${userId}`);

            if (role === 'admin') {
                admins.add(socket.id);
                socket.join('admins');
            }
        }

        // Handle sending messages
        socket.on('send_message', (data) => {
            // data = { toUserId (optional if from user), content }

            if (role === 'admin') {
                // Admin sending to specific User
                if (data.toUserId) {
                    io.to(`user_${data.toUserId}`).emit('receive_message', {
                        from: 'Admin',
                        content: data.content,
                        timestamp: new Date()
                    });
                }
            } else {
                // User sending to Admin(s)
                io.to('admins').emit('receive_message', {
                    fromUserId: userId,
                    fromUsername: username,
                    content: data.content,
                    timestamp: new Date()
                });
            }
        });

        // Typing indicators
        socket.on('typing', (data) => {
            if (role === 'user') {
                io.to('admins').emit('user_typing', { userId, username });
            } else if (data.toUserId) {
                io.to(`user_${data.toUserId}`).emit('admin_typing');
            }
        });

        socket.on('disconnect', () => {
            if (role === 'admin') {
                admins.delete(socket.id);
            }
            console.log(`[Socket] Disconnected: ${username}`);
        });
    });
};

module.exports = ChatService;
