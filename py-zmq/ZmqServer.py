import zmq
import random
import time


class ZmqServer:
    def __init__(self, pub_port, rep_port):
        self.rep_port = rep_port
        self.pub_port = pub_port

    def run(self):
        context = zmq.Context()

        poller = zmq.Poller()

        rep_socket = context.socket(zmq.REP)
        rep_socket.bind('tcp://127.0.0.1:%s' % self.rep_port)
        poller.register(rep_socket, zmq.POLLIN)

        pub_socket = context.socket(zmq.PUB)
        pub_socket.bind('tcp://127.0.0.1:%s' % self.pub_port)
        poller.register(pub_socket, zmq.POLLIN)

        print('Server started')

        while True:
            socks = dict(poller.poll())

            if rep_socket in socks and socks[rep_socket] == zmq.POLLIN:
                try:
                    message = rep_socket.recv_string(zmq.NOBLOCK)
                    rep_socket.send_string(message)
                    pub_msg = ['MESSAGE'.encode('utf-8'), message.encode('utf-8')]
                    print('Sending: {0}', message)
                    pub_socket.send_multipart(pub_msg)
                except zmq.error.Again:
                    pass


def main():
    server = ZmqServer(61044, 8888)
    server.run()


if __name__ == '__main__':
    main()
