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
  A2 
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
bool active = false;
enum OutputMode {
  Binary,     // Communication with the desktop application
  PlainText,  // Plain text, useful for the Arduino Serial Monitor
  Plotter     // Graph values, for the Arduino Serial Plotter
};
OutputMode outputMode = Binary;

byte analogValue[AnalogInputCount];
unsigned long lastChange[AnalogInputCount];
int analogReadValue[AnalogInputCount];
float emaValue[AnalogInputCount];
unsigned long currentTime;
unsigned long lastPlot;


void setup() 
{
  Serial.begin(115200);

  // Wait for the Serial port hardware to initialise
  while (!Serial) {}  


  // Seed the moving average
  for (byte analogInputIndex = 0; analogInputIndex < AnalogInputCount; analogInputIndex++)
  {
    pinMode(AnalogInputPin[analogInputIndex], INPUT);
    emaValue[analogInputIndex] = analogRead(AnalogInputPin[analogInputIndex]);
  }

  for (byte seed = 1; seed < EMASeedCount - 1; seed++)
    for (byte analogInputIndex = 0; analogInputIndex < AnalogInputCount; analogInputIndex++)
      getAnalogValue(analogInputIndex);


  // Read the initial values
  currentTime = millis();  
  for (byte analogInputIndex = 0; analogInputIndex < AnalogInputCount; analogInputIndex++)
  {
    analogValue[analogInputIndex] = getAnalogValue(analogInputIndex);
    lastChange[analogInputIndex] = currentTime;
  }
}


void loop() 
{
  if (Serial.available())
    processMessage(Serial.read());

  // Not that due to ADC checking and Serial communication, currentTime will not be
  // accurate throughout the loop. But since we don't need exact timing for the interval this
  // is acceptable and saves a few calls to millis.
  currentTime = millis();

  // Check analog inputs
  byte newAnalogValue;
  for (byte analogInputIndex = 0; analogInputIndex < AnalogInputCount; analogInputIndex++)
  {
    newAnalogValue = getAnalogValue(analogInputIndex);

    if (newAnalogValue != analogValue[analogInputIndex] && (currentTime - lastChange[analogInputIndex] >= MinimumInterval))
    {
      if (active)
        // Send out new value
        outputAnalogValue(analogInputIndex, newAnalogValue);

      analogValue[analogInputIndex] = newAnalogValue;
      lastChange[analogInputIndex] = currentTime;
    }
  }

  if (outputMode == Plotter && (currentTime - lastPlot) >= 50)
  {
    outputPlotter();
    lastPlot = currentTime;
  }
}


void processMessage(byte message)
{
  switch (message)
  {
    case 'H':   // Handshake
      processHandshakeMessage();
      break;

    case 'Q':   // Quit
      processQuitMessage();
      break;

    default:
      outputError("Unknown message: " + (char)message);
      break;
  }
}


void processHandshakeMessage()
{
  byte buffer[3];
  if (Serial.readBytes(buffer, 3) < 3)
  {
    outputError("Invalid handshake length");
    return;
  }
  
  if (buffer[0] != 'M' || buffer[1] != 'K')
  {
    outputError("Invalid handshake: " + String((char)buffer[0]) + String((char)buffer[1]) + String((char)buffer[2]));
    return;
  }

  switch (buffer[2])
  {
    case 'B':
      outputMode = Binary;
      break;

    case 'P':
      outputMode = PlainText;
      break;

    case 'G':
      outputMode = Plotter;
      break;

    default:
      outputMode = PlainText;
      outputError("Unknown output mode: " + String((char)buffer[2]));
      return;
  }


  switch (outputMode)
  {
    case Binary:
      Serial.write('H');
      Serial.write(AnalogInputCount);
      Serial.write((byte)0);
      Serial.write((byte)0);
      Serial.write((byte)0);
      break;

    case PlainText:
      Serial.print("Hello! I have ");
      Serial.print(AnalogInputCount);
      Serial.println(" analog inputs and no support yet for everything else.");
      break;
  }

  active = true;
}


void processQuitMessage()
{
  switch (outputMode)
  {
    case Binary:
    case PlainText:
      Serial.write('Q');
      break;
  }
  
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
  switch (outputMode)
  {
    case Binary:
      Serial.write('V');
      Serial.write(analogInputIndex);
      Serial.write(newValue);
      break;

    case PlainText:
      Serial.print("Analog value #");
      Serial.print(analogInputIndex);
      Serial.print(" = ");
      Serial.println(newValue);
      break;
  }
}


void outputPlotter()
{
  for (byte i = 0; i < AnalogInputCount; i++)
  {
    if (i > 0)
      Serial.print('\t');

    Serial.print(analogReadValue[i]);
    Serial.print('\t');
    Serial.print(emaValue[i]);
    Serial.print('\t');
    Serial.print(analogValue[i]);
  }

  Serial.println();
}


void outputError(String message)
{
  switch (outputMode)
  {   
    case Binary:
      Serial.write('E');
      Serial.write((byte)message.length());
      Serial.print(message);
      break;
    
    case PlainText:
      Serial.print("Error: ");
      Serial.println(message);
      break;
  }
}
