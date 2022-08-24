from concurrent import futures
from re import X
from time import sleep
from tkinter import SOLID
from turtle import color
import enum
import grpc
import frame_pb2
import frame_pb2_grpc

class Color(enum.IntEnum):
    COLOR_UNKNOWN = 0
    RED = 1
    BLUE = 2

class Style(enum.IntEnum):
    STYLE_UNKNOWN = 0
    SOLID = 1
    DASHED = 2

def SetFrameData(frame, color, style, x1, y1, x2, y2):
    print(color)
    frame.color = color
    print(style)
    frame.style = style
    print(style)
    frame.x1 = x1
    frame.y1 = y1
    frame.x2 = x2
    frame.y2 = y2
    print(frame.x1)

class FrameServicer(frame_pb2_grpc.FrameService):
    def ReceiveFrameDataStream(self, request, context):
        print("stream")
        frameData = frame_pb2.FrameData()
        frame1 = frame_pb2.Frame()
        SetFrameData(frame1, Color.RED.value, Style.SOLID.value, 0.1, 0.2, 0.35, 0.5)
        print("stream")
        frameData.frames.append(frame1)
        frame2 = frame_pb2.Frame()
        SetFrameData(frame2, Color.BLUE.value, Style.DASHED.value, 0.5, 0.5, 0.8, 0.7)
        frameData.frames.append(frame2)
        varX=0.01
        varY=0.01
        while True:
            if (frameData.frames[0].x2 > 1):
                varX = -0.02
            if (frameData.frames[0].x1 < 0):
                varX = 0.02
            if (frameData.frames[0].y2 > 1):
                varY = -0.02
            if (frameData.frames[0].y1 < 0):
                varY = 0.02
            if (frameData.frames[1].x2 > 1):
                frameData.frames[1].x1 = 0.5
                frameData.frames[1].y1 = 0.5
                frameData.frames[1].x2 = 0.8
                frameData.frames[1].y2 = 0.7
            frameData.frames[0].x1 = frameData.frames[0].x1 + varX
            frameData.frames[0].x2 = frameData.frames[0].x2 + varX
            frameData.frames[0].y1 = frameData.frames[0].y1 + varY
            frameData.frames[0].y2 = frameData.frames[0].y2 + varY
            frameData.frames[1].x1 = frameData.frames[1].x1 + 0.015
            frameData.frames[1].x2 = frameData.frames[1].x2 + 0.015
            frameData.frames[1].y1 = frameData.frames[1].y1 - 0.01
            frameData.frames[1].y2 = frameData.frames[1].y2 - 0.01
            yield frame_pb2.FrameData(num = 2, frames = frameData.frames)
            print("yield... ", frameData.frames[1].x1)
            sleep(0.05)

def serve():
    server = grpc.server(futures.ThreadPoolExecutor(max_workers=10))
    frame_pb2_grpc.add_FrameServiceServicer_to_server(FrameServicer(), server)
    server.add_insecure_port('127.0.0.1:11051')
    server.start()
    print('server start.')
    server.wait_for_termination()

if __name__ == '__main__':
    serve()