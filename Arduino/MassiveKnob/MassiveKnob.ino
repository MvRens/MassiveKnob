/*
 * 
 * Configuration
 *   Modify these settings according to your hardware design
 * 
 */
// Set this to the number of potentiometers you have connected
const byte KnobCount = 1;

// For each potentiometer, specify the port
const byte KnobPin[KnobCount] = {
//  A0,
  A1
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

byte volume[KnobCount];
unsigned long lastChange[KnobCount];
int analogReadValue[KnobCount];
float emaValue[KnobCount];
unsigned long currentTime;
unsigned long lastPlot;

void setup() 
{
  Serial.begin(115200);

  // Wait for the Serial port hardware to initialise
  while (!Serial) {}  


  // Seed the moving average
  for (byte knobIndex = 0; knobIndex < KnobCount; knobIndex++)
    emaValue[knobIndex] = analogRead(KnobPin[knobIndex]);

  for (byte seed = 1; seed < EMASeedCount - 1; seed++)
    for (byte knobIndex = 0; knobIndex < KnobCount; knobIndex++)
      getVolume(knobIndex);


  // Read the initial values
  currentTime = millis();  
  for (byte knobIndex = 0; knobIndex < KnobCount; knobIndex++)
  {
    volume[knobIndex] = getVolume(knobIndex);
    lastChange[knobIndex] = currentTime;
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

  // Check volume knobs  
  byte newVolume;
  for (byte knobIndex = 0; knobIndex < KnobCount; knobIndex++)
  {
    newVolume = getVolume(knobIndex);

    if (newVolume != volume[knobIndex] && (currentTime - lastChange[knobIndex] >= MinimumInterval))
    {
      if (active)
        // Send out new value
        outputVolume(knobIndex, newVolume);

      volume[knobIndex] = newVolume;
      lastChange[knobIndex] = currentTime;
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
  }
}


void processHandshakeMessage()
{
  byte buffer[2];
  if (Serial.readBytes(buffer, 3) < 3)
    return;
  
  if (buffer[0] != 'M' || buffer[1] != 'K')
    return;

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
      return;
  }


  switch (outputMode)
  {
    case Binary:
      Serial.write('H');
      Serial.write(KnobCount);
      break;

    case PlainText:
      Serial.print("Hello! I have ");
      Serial.print(KnobCount);
      Serial.println(" knobs.");
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


byte getVolume(byte knobIndex)
{
  analogReadValue[knobIndex] = analogRead(KnobPin[knobIndex]);
  emaValue[knobIndex] = (EMAAlpha * analogReadValue[knobIndex]) + ((1 - EMAAlpha) * emaValue[knobIndex]);
  
  return map(emaValue[knobIndex], 0, 1023, 0, 100);
}


void outputVolume(byte knobIndex, byte newVolume)
{
  switch (outputMode)
  {
    case Binary:
      Serial.write('V');
      Serial.write(knobIndex);
      Serial.write(newVolume);    
      break;

    case PlainText:
      Serial.print("Volume #");
      Serial.print(knobIndex);
      Serial.print(" = ");
      Serial.println(newVolume);
      break;
  }
}


void outputPlotter()
{
  for (byte i = 0; i < KnobCount; i++)
  {
    if (i > 0)
      Serial.print('\t');

    Serial.print(analogReadValue[i]);
    Serial.print('\t');
    Serial.print(emaValue[i]);
    Serial.print('\t');
    Serial.print(volume[i]);
  }

  Serial.println();
}
