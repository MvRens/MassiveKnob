/*
 * 
 * Configuration
 *   Modify these settings according to your hardware design
 * 
 */
// Set this to the number of potentiometers you have connected
const byte AnalogInputCount = 1;

// For each potentiometer, specify the port
const byte AnalogInputPin[AnalogInputCount] = {
  A0
};

// Minimum time between reporting changing values, reduces serial traffic
const unsigned long MinimumInterval = 50;

// Alpha value of the Exponential Moving Average (EMA) to reduce noise
const float EMAAlpha = 0.6;

// How many measurements to take at boot time to seed the EMA
const byte EMASeedCount = 5;




/*
 * 
 * Le code
 *   Here be dragons.
 * 
 */
#include "./min.h"
#include "./min.c"


struct min_context minContext;

uint16_t min_tx_space(uint8_t port) { return 512U; }
void min_tx_byte(uint8_t port, uint8_t byte) 
{
  while (Serial.write(&byte, 1U) == 0) { }
}
uint32_t min_time_ms(void) { return millis(); }
void min_application_handler(uint8_t min_id, uint8_t *min_payload, uint8_t len_payload, uint8_t port);
void min_tx_start(uint8_t port) {}
void min_tx_finished(uint8_t port) {}

const uint8_t FrameIDHandshake = 42;
const uint8_t FrameIDHandshakeResponse = 43;
const uint8_t FrameIDAnalogInput = 1;
const uint8_t FrameIDDigitalInput = 2;
const uint8_t FrameIDAnalogOutput = 3;
const uint8_t FrameIDDigitalOutput = 4;
const uint8_t FrameIDQuit = 62;
const uint8_t FrameIDError = 63;


bool active = false;

byte analogValue[AnalogInputCount];
unsigned long lastChange[AnalogInputCount];
int analogReadValue[AnalogInputCount];
float emaValue[AnalogInputCount];
unsigned long lastPlot;


void setup() 
{
  Serial.begin(115200);

  // Wait for the Serial port hardware to initialise
  while (!Serial) {}  


  // Set up the MIN protocol (http://github.com/min-protocol/min)
  min_init_context(&minContext, 0);


  // Seed the moving average
  for (byte analogInputIndex = 0; analogInputIndex < AnalogInputCount; analogInputIndex++)
  {
    pinMode(AnalogInputPin[analogInputIndex], INPUT);
    emaValue[analogInputIndex] = analogRead(AnalogInputPin[analogInputIndex]);
  }

  for (byte analogInputIndex = 0; analogInputIndex < AnalogInputCount; analogInputIndex++)
    for (byte seed = 1; seed < EMASeedCount - 1; seed++)
      getAnalogValue(analogInputIndex);


  // Read the initial values
  for (byte analogInputIndex = 0; analogInputIndex < AnalogInputCount; analogInputIndex++)
  {
    analogValue[analogInputIndex] = getAnalogValue(analogInputIndex);
    lastChange[analogInputIndex] = millis();
  }
}


void loop() 
{
  char readBuffer[32];
  size_t readBufferSize = Serial.available() > 0 ? Serial.readBytes(readBuffer, 32U) : 0;
  
  min_poll(&minContext, (uint8_t*)readBuffer, (uint8_t)readBufferSize);

 
  // Check analog inputs
  byte newAnalogValue;
  for (byte analogInputIndex = 0; analogInputIndex < AnalogInputCount; analogInputIndex++)
  {
    newAnalogValue = getAnalogValue(analogInputIndex);

    if (newAnalogValue != analogValue[analogInputIndex] && (millis() - lastChange[analogInputIndex] >= MinimumInterval))
    {
      if (active)
        // Send out new value
        outputAnalogValue(analogInputIndex, newAnalogValue);

      analogValue[analogInputIndex] = newAnalogValue;
      lastChange[analogInputIndex] = millis();
    }
  }
}


void min_application_handler(uint8_t min_id, uint8_t *min_payload, uint8_t len_payload, uint8_t port)
{
  switch (min_id)
  {
    case FrameIDHandshake:
      processHandshakeMessage(min_payload, len_payload);
      break;
      
    case FrameIDAnalogOutput:
      //processAnalogOutputMessage();
      break;
      
    case FrameIDDigitalOutput:
      //processDigitalOutputMessage();
      break;
      
    case FrameIDQuit:
      processQuitMessage();
      break;

    default:
      outputError("Unknown frame ID: " + String(min_id));
      break;      
  }
}

void processHandshakeMessage(uint8_t *min_payload, uint8_t len_payload)
{
  if (len_payload < 2)
  {
    outputError("Invalid handshake length");
    return;
  }
  
  if (min_payload[0] != 'M' || min_payload[1] != 'K')
  {
    outputError("Invalid handshake: " + String((char)min_payload[0]) + String((char)min_payload[1]));
    return;
  }

  byte payload[4] { AnalogInputCount, 0, 0, 0 };
  if (min_queue_frame(&minContext, FrameIDHandshakeResponse, (uint8_t *)payload, 4))
    active = true;
}


void processQuitMessage()
{ 
  active = false;
}


byte getAnalogValue(byte analogInputIndex)
{
  analogRead(AnalogInputPin[analogInputIndex]);
  
  // Give the ADC some time to stabilize
  delay(10);
  
  analogReadValue[analogInputIndex] = analogRead(AnalogInputPin[analogInputIndex]);
  emaValue[analogInputIndex] = (EMAAlpha * analogReadValue[analogInputIndex]) + ((1 - EMAAlpha) * emaValue[analogInputIndex]);
  
  return map(emaValue[analogInputIndex], 0, 1023, 0, 100);
}


void outputAnalogValue(byte analogInputIndex, byte newValue)
{
  byte payload[2] = { analogInputIndex, newValue };
  min_send_frame(&minContext, FrameIDAnalogInput, (uint8_t *)payload, 2);
}


void outputError(String message)
{
  min_send_frame(&minContext, FrameIDError, (uint8_t *)message.c_str(), message.length());
}
