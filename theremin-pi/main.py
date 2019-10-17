import RPi.GPIO as GPIO
import time

SPICLK = 18
SPIMISO = 23
SPIMOSI = 24
SPICS = 25
SPKER = 5

adc_in = 1

freq = [130.813, 138.591, 146.832, 155.563, 164.814,
        174.614, 184.997, 195.998, 207.652, 220.000,
        233.082, 246.942, 261.626, 277.183, 293.665,
        311.127, 329.628, 349.228, 369.994, 391.995,
        415.305, 440.000, 466.164, 493.883, 523.251]

GPIO.setmode(GPIO.BCM)

GPIO.setup(SPIMOSI, GPIO.OUT)
GPIO.setup(SPIMISO, GPIO.IN)
GPIO.setup(SPICLK, GPIO.OUT)
GPIO.setup(SPICS, GPIO.OUT)
GPIO.setup(SPKER, GPIO.OUT)

spker_out = GPIO.PWM(SPKER, freq[13])


def readadc(adcnum, clockpin, mosipin, misopin, cspin):
    if (adcnum > 7) or (adcnum < 0):
        return -1

    GPIO.output(cspin, True)
    GPIO.output(clockpin, False)
    GPIO.output(cspin, False)

    commandout = adcnum
    commandout |= 0x18  # start bit + single-ended bit
    commandout <<= 3  # we only need to send 5 bits here

    for i in range(5):
        if commandout & 0x80:
            GPIO.output(mosipin, True)
        else:
            GPIO.output(mosipin, False)
        commandout <<= 1
        GPIO.output(clockpin, True)
        GPIO.output(clockpin, False)

    adcout = 0

    # read in one empty bit, one null bit and 10 ADC bits
    for i in range(12):
        GPIO.output(clockpin, True)
        GPIO.output(clockpin, False)
        adcout <<= 1
        if GPIO.input(misopin):
            adcout |= 0x1

    GPIO.output(cspin, True)

    adcout >>= 1  # first bit is 'null' so drop it
    return adcout


last_read = 0  # this keeps track of the last potentiometer value
tolerance = 5  # to keep from being jittery we'll only change

spker_out.start(0.5)

try:
    while True:
        adc_changed = False

        # read the analog pin
        raw_value = readadc(adc_in, SPICLK, SPIMOSI, SPIMISO, SPICS)

        # how much has it changed since the last read?
        adc_delta = abs(raw_value - last_read)

        if adc_delta > tolerance:
            adc_changed = True

        if adc_changed:
            freq_idx = int(round(raw_value / 40.96))
            spker_out.ChangeFrequency(freq[freq_idx])

        time.sleep(0.1)
except KeyboardInterrupt:
    pass
finally:
    spker_out.stop()
    GPIO.cleanup()
