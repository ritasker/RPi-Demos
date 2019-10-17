import RPi.GPIO as GPIO
import time

GPIO.setmode(GPIO.BCM)
GPIO.setup(24, GPIO.IN)
GPIO.setup(25, GPIO.OUT)

try:
    while True:
        inputValue = GPIO.input(24)
        if inputValue:
            GPIO.output(25, GPIO.HIGH)
            time.sleep(0.1)
        else:
            GPIO.output(25, GPIO.LOW)
            time.sleep(0.1)
except KeyboardInterrupt:
    pass
finally:
    GPIO.cleanup()
