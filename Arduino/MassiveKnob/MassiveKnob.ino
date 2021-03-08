/*
 * 
 * Configuration
 *   Modify these settings according to your hardware design
 * 
 */
// Set this to the number of potentiometers you have connected
const byte AnalogInputCount = 2;

// Set this to the number of buttons you have connected
const byte DigitalInputCount = 3;

// Set this to the number of PWM outputs you have connected
// Note that this version of the sketch only does a simple analogWrite with the full range,
// which is not compatible with servos. Modify as required.
const byte AnalogOutputCount = 3;

// Set this to the number of digital outputs you have connected
const byte DigitalOutputCount = 0;


// For each potentiometer, specify the pin
const byte AnalogInputPin[AnalogInputCount] = {
  A0,
  A1
};

// For each button, specify the pin. Assumes pull-up.
const byte DigitalInputPin[DigitalInputCount] = {
  7,
  8,
  9
};

// For each analog output, specify the PWM capable pin
const byte AnalogOutputPin[AnalogOutputCount] = {
  3,
  5,
  6
};

// Define this constant to apply a standard LED brightness curve to (all) analog outputs
#define AnalogOutputGammaCorrection


// For each digital output, specify the pin
const byte DigitalOutputPin[DigitalOutputCount] = {
};


// Minimum time between reporting changing values, reduces serial traffic and debounces digital inputs
const unsigned long MinimumInterval = 50;

// Alpha value of the Exponential Moving Average (EMA) for analog inputs to reduce noise
const float EMAAlpha = 0.6;

// How many measurements to take at boot time for analog inputs to seed the EMA
const byte EMASeedCount = 5;

// Minimum treshold for reporting changes in analog values, reduces noise left over from the EMA. Note that once an analog value 
// changes beyond the treshold, that input will report all changes until the FocusTimeout has expired to avoid losing accuracy.
const byte AnalogTreshold = 2;

// How long to ignore other inputs after an input changes. Reduces noise due voltage drops.
const unsigned long FocusTimeout = 100;


/*
 * 
 * Le code
 *   Here be dragons.
 * 
 */

// If defined, only outputs will be sent to the serial port as Arduino Plotter compatible data
//#define DebugOutputPlotter


#ifndef DebugOutputPlotter
#include "./min.h"
#include "./min.c"


// MIN protocol context and callbacks
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
#endif


struct AnalogInputStatus
{
  byte Value;
  unsigned long LastChange;
  int ReadValue;
  float EMAValue;
};

struct DigitalInputStatus
{
  bool Value;
  unsigned long LastChange;
};


struct AnalogInputStatus analogInputStatus[AnalogInputCount];
struct DigitalInputStatus digitalInputStatus[DigitalInputCount];


void setup() 
{
  Serial.begin(115200);

  // Wait for the Serial port hardware to initialise
  while (!Serial) {}  


  #ifndef DebugOutputPlotter
  // Set up the MIN protocol (http://github.com/min-protocol/min)
  min_init_context(&minContext, 0);
  #endif


  // Seed the moving average for analog inputs
  for (byte i = 0; i < AnalogInputCount; i++)
  {
    pinMode(AnalogInputPin[i], INPUT);
    analogInputStatus[i].EMAValue = analogRead(AnalogInputPin[i]);
  }

  for (byte i = 0; i < AnalogInputCount; i++)
    for (byte seed = 1; seed < EMASeedCount - 1; seed++)
      getAnalogValue(i);


  // Read the initial stabilized values
  for (byte i = 0; i < AnalogInputCount; i++)
  {
    analogInputStatus[i].Value = getAnalogValue(i);
    analogInputStatus[i].LastChange = millis();
  }

  // Set up analog outputs
  for (byte i = 0; i < AnalogOutputCount; i++)
  {
    pinMode(AnalogOutputPin[i], OUTPUT);
    analogWrite(AnalogOutputPin[i], 0);
  }


  // Set up digital inputs and outputs
  for (byte i = 0; i < DigitalInputCount; i++)
  {
    pinMode(DigitalInputPin[i], INPUT_PULLUP);
    digitalInputStatus[i].Value = getDigitalValue(i);
    digitalInputStatus[i].LastChange = millis();
  }

  for (byte i = 0; i < DigitalOutputCount; i++)
  {
    pinMode(DigitalOutputPin[i], OUTPUT);
    digitalWrite(DigitalOutputPin[i], LOW);
  }
}


#ifdef DebugOutputPlotter
unsigned long lastOutput = 0;
#endif

enum FocusType
{
  FocusTypeNone = 0,
  FocusTypeAnalogInput = 1,
  FocusTypeDigitalInput = 2,
  FocusTypeOutput = 3
};

bool active = false;
FocusType focusType = FocusTypeNone;
byte focusInputIndex;
unsigned long focusOutputTime;

#define IsAnalogInputFocus(i) ((focusType == FocusInputType.AnalogInput) && (focusInputIndex == i))
#define IsDigitalInputFocus(i) ((focusType == FocusInputType.DigitalInput) && (focusInputIndex == i))


#ifdef AnalogOutputGammaCorrection
const uint8_t PROGMEM gamma8[] = {
    0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
    0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  1,  1,  1,  1,
    1,  1,  1,  1,  1,  1,  1,  1,  1,  2,  2,  2,  2,  2,  2,  2,
    2,  3,  3,  3,  3,  3,  3,  3,  4,  4,  4,  4,  4,  5,  5,  5,
    5,  6,  6,  6,  6,  7,  7,  7,  7,  8,  8,  8,  9,  9,  9, 10,
   10, 10, 11, 11, 11, 12, 12, 13, 13, 13, 14, 14, 15, 15, 16, 16,
   17, 17, 18, 18, 19, 19, 20, 20, 21, 21, 22, 22, 23, 24, 24, 25,
   25, 26, 27, 27, 28, 29, 29, 30, 31, 32, 32, 33, 34, 35, 35, 36,
   37, 38, 39, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 50,
   51, 52, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63, 64, 66, 67, 68,
   69, 70, 72, 73, 74, 75, 77, 78, 79, 81, 82, 83, 85, 86, 87, 89,
   90, 92, 93, 95, 96, 98, 99,101,102,104,105,107,109,110,112,114,
  115,117,119,120,122,124,126,127,129,131,133,135,137,138,140,142,
  144,146,148,150,152,154,156,158,160,162,164,167,169,171,173,175,
  177,180,182,184,186,189,191,193,196,198,200,203,205,208,210,213,
  215,218,220,223,225,228,231,233,236,239,241,244,247,249,252,255 };
#endif


void loop() 
{
  #ifndef DebugOutputPlotter
  char readBuffer[32];
  size_t readBufferSize = Serial.available() > 0 ? Serial.readBytes(readBuffer, 32U) : 0;
  
  min_poll(&minContext, (uint8_t*)readBuffer, (uint8_t)readBufferSize);
  #endif


  if (focusType == FocusTypeOutput && millis() - focusOutputTime >= FocusTimeout)
    focusType = FocusTypeNone;

 
  // Check analog inputs
  byte newAnalogValue;
  for (byte i = 0; i < AnalogInputCount; i++)
  {
    newAnalogValue = getAnalogValue(i);
    bool changed;

    switch (focusType)
    {
      case FocusTypeAnalogInput:
        if (focusInputIndex != i)
          continue;

        if (millis() - analogInputStatus[i].LastChange < FocusTimeout)
        {
          changed = newAnalogValue != analogInputStatus[i].Value;
          break;
        }
        else
          focusType = FocusTypeNone;
          // fall-through
          
      case FocusTypeNone:
        changed = abs(analogInputStatus[i].Value - newAnalogValue) >= AnalogTreshold;
        break;

      default:
        continue;
    }

    if (changed && (millis() - analogInputStatus[i].LastChange >= MinimumInterval))
    {
      if (active)
        // Send out new value
        outputAnalogValue(i, newAnalogValue);

      analogInputStatus[i].Value = newAnalogValue;
      analogInputStatus[i].LastChange = millis();
    }
  }


  // Check digital inputs
  bool newDigitalValue;
  for (byte i = 0; i < DigitalInputCount; i++)
  {
    newDigitalValue = getDigitalValue(i);

    switch (focusType)
    {
      case FocusTypeAnalogInput:
      case FocusTypeOutput:
        continue;
        
      case FocusTypeDigitalInput:
        if (focusInputIndex != i)
          continue;

        if (millis() - digitalInputStatus[i].LastChange >= FocusTimeout)
          focusType = FocusTypeNone;

        break;
    }

    
    if (newDigitalValue != digitalInputStatus[i].Value && (millis() - digitalInputStatus[i].LastChange >= MinimumInterval))
    {
      if (active)
        // Send out new value
        outputDigitalValue(i, newDigitalValue);

      digitalInputStatus[i].Value = newDigitalValue;
      digitalInputStatus[i].LastChange = millis();
    }
  }

  #ifdef DebugOutputPlotter
  if (millis() - lastOutput >= 100)
  {
    for (byte i = 0; i < AnalogInputCount; i++)
    {
      if (i > 0)
        Serial.print("\t");

      Serial.print(analogInputStatus[i].Value);
    }
    
    for (byte i = 0; i < DigitalInputCount; i++)
    {
      if (i > 0 || AnalogInputCount > 0)
        Serial.print("\t");

      Serial.print(digitalInputStatus[i].Value ? 100 : 0);
    }

    Serial.println();

    lastOutput = millis();
  }
  #endif
}


#ifndef DebugOutputPlotter
void min_application_handler(uint8_t min_id, uint8_t *min_payload, uint8_t len_payload, uint8_t port)
{
  switch (min_id)
  {
    case FrameIDHandshake:
      processHandshakeMessage(min_payload, len_payload);
      break;
      
    case FrameIDAnalogOutput:
      processAnalogOutputMessage(min_payload, len_payload);
      break;
      
    case FrameIDDigitalOutput:
      processDigitalOutputMessage(min_payload, len_payload);
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

  byte payload[4] { AnalogInputCount, DigitalInputCount, AnalogOutputCount, DigitalOutputCount };
  if (min_queue_frame(&minContext, FrameIDHandshakeResponse, (uint8_t *)payload, 4))
    active = true;
}


void processDigitalOutputMessage(uint8_t *min_payload, uint8_t len_payload)
{
  if (len_payload < 2)
  {
    outputError("Invalid digital output payload length");
    return;
  }

  byte outputIndex = min_payload[0];
  if (outputIndex < DigitalOutputCount)
  {
    digitalWrite(DigitalOutputPin[min_payload[0]], min_payload[1] == 0 ? LOW : HIGH);

    focusType = FocusTypeOutput;
    focusOutputTime = millis();
  }
  else
    outputError("Invalid digital output index: " + String(outputIndex));
}


void processAnalogOutputMessage(uint8_t *min_payload, uint8_t len_payload)
{
  if (len_payload < 2)
  {
    outputError("Invalid analog output payload length");
    return;
  }

  byte outputIndex = min_payload[0];
  if (outputIndex < AnalogOutputCount)
  {
    byte value = min_payload[1];
    if (value > 100)
      value = 100;

    value = map(value, 0, 100, 0, 255);

    #ifdef AnalogOutputGammaCorrection
    value = pgm_read_byte(&gamma8[value]);
    #endif
    
    analogWrite(AnalogOutputPin[min_payload[0]], value);

    focusType = FocusTypeOutput;
    focusOutputTime = millis();
  }
  else
    outputError("Invalid analog output index: " + String(outputIndex));
}

void processQuitMessage()
{ 
  active = false;
}
#endif


byte getAnalogValue(byte analogInputIndex)
{
  analogRead(AnalogInputPin[analogInputIndex]);
  
  // Give the ADC some time to stabilize
  delay(10);
  
  int readValue = analogRead(AnalogInputPin[analogInputIndex]);
  analogInputStatus[analogInputIndex].ReadValue = readValue;
  
  int newEMAValue = (EMAAlpha * readValue) + ((1 - EMAAlpha) * analogInputStatus[analogInputIndex].EMAValue);
  analogInputStatus[analogInputIndex].EMAValue = newEMAValue;
  
  return map(newEMAValue, 0, 1023, 0, 100);
}


bool getDigitalValue(byte digitalInputIndex)
{
  return digitalRead(DigitalInputPin[digitalInputIndex]) == LOW;
}


void outputAnalogValue(byte analogInputIndex, byte newValue)
{
  #ifndef DebugOutputPlotter
  byte payload[2] = { analogInputIndex, newValue };
  min_send_frame(&minContext, FrameIDAnalogInput, (uint8_t *)payload, 2);
  #endif
}


void outputDigitalValue(byte digitalInputIndex, bool newValue)
{
  #ifndef DebugOutputPlotter
  byte payload[2] = { digitalInputIndex, newValue ? 1 : 0 };
  min_send_frame(&minContext, FrameIDDigitalInput, (uint8_t *)payload, 2);
  #endif
}


void outputError(String message)
{
  #ifndef DebugOutputPlotter
  min_send_frame(&minContext, FrameIDError, (uint8_t *)message.c_str(), message.length());
  #endif
}
