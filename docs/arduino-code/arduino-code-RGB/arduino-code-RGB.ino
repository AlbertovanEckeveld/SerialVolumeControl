#include <Adafruit_NeoPixel.h>

#define LED_PIN    2
#define NUM_LEDS   8

Adafruit_NeoPixel strip(NUM_LEDS, LED_PIN, NEO_GRB + NEO_KHZ800);
int currentEffect = 1; 

String inputString = "";
bool inputComplete = false;

int prevValues[5] = {0, 0, 0, 0, 0}; 
const int threshold = 2; 


void setup() {
  Serial.begin(9600);
  strip.begin();
  strip.setBrightness(100);
  strip.show();
  /*
  Serial.println("Typ 0-10 in seriële monitor om effect te kiezen:");
  Serial.println("0=Color Chase");
  Serial.println("1=Rainbow Cycle");
  Serial.println("2=Blinking Dots");
  Serial.println("3=Breathing Light");
  Serial.println("4=Fire Flicker");
  Serial.println("5=Knight Rider / Scanner");
  Serial.println("6=Sparkle");
  Serial.println("7=Gradient Fade");
  Serial.println("8=Wave");
  Serial.println("9=Larson Scanner with Trail");
  Serial.println("10=Stilstaande Kleur"); 
  */
}

void loop() {
  sendSliderData();
  if (inputComplete) {
    int newEffect = inputString.toInt();
    if (newEffect >= 0 && newEffect <= 10) {
      currentEffect = newEffect;
    }

    inputString = "";
    inputComplete = false;
  }

  switch (currentEffect) {
    case 0: effectColorChase(strip.Color(0, 255, 255)); break;
    case 1: effectRainbowCycle(); break;
    case 2: effectBlinkingDots(); break;
    case 3: effectBreathingLight(strip.Color(0, 0, 255)); break;
    case 4: effectFireFlicker(); break;
    case 5: effectKnightRider(); break;
    case 6: effectSparkle(strip.Color(0, 0, 100)); break;
    case 7: effectGradientFade(); break;
    case 8: effectWave(); break;
    case 9: effectLarsonScannerTrail(); break;
    case 10: effectStaticColor(strip.Color(255, 0, 0)); break;  // rode stilstaande kleur
    default: effectColorChase(strip.Color(0, 255, 255)); break;
  }
}

void sendSliderData() {
  int currentValues[5];

  for (int i = 0; i < 5; i++) {
    currentValues[i] = analogRead(A0 + i);

    if (abs(currentValues[i] - prevValues[i]) > threshold) {
      Serial.print("slider");
      Serial.print(i);
      Serial.print(": ");
      Serial.println(currentValues[i]);
      prevValues[i] = currentValues[i];
    }
  }
}

void serialEvent() {
  while (Serial.available()) {
    char inChar = (char)Serial.read();
    if (inChar == '\n') {
      inputComplete = true;
    } else if (inChar != '\r') {
      inputString += inChar;
    }
  }
}

void effectColorChase(uint32_t color) {
  static int pos = 0;
  for (int i = 0; i < NUM_LEDS; i++) {
    strip.setPixelColor(i, (i == pos) ? color : 0);
  }
  strip.show();
  pos = (pos + 1) % NUM_LEDS;
  delay(100);
}

void effectRainbowCycle() {
  static uint16_t j = 0;
  for (int i = 0; i < NUM_LEDS; i++) {
    int pos = (i * 256 / NUM_LEDS + j) & 255;
    strip.setPixelColor(i, Wheel(pos));
  }
  strip.show();
  j = (j + 1) & 255;
  delay(20);
}

void effectBlinkingDots() {
  for (int i = 0; i < NUM_LEDS; i++) {
    if (random(20) < 2) {
      strip.setPixelColor(i, strip.Color(255, 255, 255));
    } else {
      strip.setPixelColor(i, 0);
    }
  }
  strip.show();
  delay(150);
}

void effectBreathingLight(uint32_t baseColor) {
  static uint8_t brightness = 0;
  static int8_t direction = 1;

  brightness += direction * 5;
  if (brightness == 0 || brightness == 255) direction = -direction;

  uint8_t r = ((baseColor >> 16) & 0xFF) * brightness / 255;
  uint8_t g = ((baseColor >> 8) & 0xFF) * brightness / 255;
  uint8_t b = (baseColor & 0xFF) * brightness / 255;

  for (int i = 0; i < NUM_LEDS; i++) {
    strip.setPixelColor(i, strip.Color(r, g, b));
  }
  strip.show();
  delay(30);
}

void effectFireFlicker() {
  for (int i = 0; i < NUM_LEDS; i++) {
    byte r = random(180, 255);
    byte g = random(30, 80);
    byte b = 0;
    strip.setPixelColor(i, strip.Color(r, g, b));
  }
  strip.show();
  delay(100);
}

void effectKnightRider() {
  static int pos = 0;
  static int dir = 1;

  for (int i = 0; i < NUM_LEDS; i++) {
    if (i == pos)
      strip.setPixelColor(i, strip.Color(255, 0, 0));
    else
      strip.setPixelColor(i, strip.Color(10, 0, 0));
  }
  strip.show();

  pos += dir;
  if (pos == 0 || pos == NUM_LEDS - 1) dir = -dir;

  delay(60);
}

void effectSparkle(uint32_t baseColor) {
  for (int i = 0; i < NUM_LEDS; i++) {
    if (random(20) < 1) {
      strip.setPixelColor(i, strip.Color(255, 255, 255));
    } else {
      strip.setPixelColor(i, baseColor);
    }
  }
  strip.show();
  delay(100);
}

void effectGradientFade() {
  static uint8_t step = 0;
  static uint8_t r = 255, g = 0, b = 0;

  switch (step) {
    case 0: if (++g == 255) step = 1; break;
    case 1: if (--r == 0) step = 2; break;
    case 2: if (++b == 255) step = 3; break;
    case 3: if (--g == 0) step = 4; break;
    case 4: if (++r == 255) step = 5; break;
    case 5: if (--b == 0) step = 0; break;
  }

  for (int i = 0; i < NUM_LEDS; i++) {
    strip.setPixelColor(i, strip.Color(r, g, b));
  }
  strip.show();
  delay(20);
}

void effectWave() {
  float t = millis() * 0.004;
  for (int i = 0; i < NUM_LEDS; i++) {
    float wave = (i * 0.5) + t;
    byte r = constrain((sin(wave + 0.0) * 127) + 127, 0, 255);
    byte g = constrain((sin(wave + 2.1) * 127) + 127, 0, 255);
    byte b = constrain((sin(wave + 4.2) * 127) + 127, 0, 255);
    strip.setPixelColor(i, strip.Color(r, g, b));
  }
  strip.show();
  delay(25);
}

void effectLarsonScannerTrail() {
  static int pos = 0;
  static int dir = 1;
  static uint8_t trail[NUM_LEDS] = {0};

  for (int i = 0; i < NUM_LEDS; i++) {
    if (trail[i] > 10) trail[i] -= 10;
    else trail[i] = 0;
  }

  trail[pos] = 255;

  for (int i = 0; i < NUM_LEDS; i++) {
    uint8_t brightness = trail[i];
    strip.setPixelColor(i, strip.Color(brightness, 0, 0));
  }

  strip.show();

  pos += dir;
  if (pos == 0 || pos == NUM_LEDS - 1) dir = -dir;

  delay(40);
}

void effectStaticColor(uint32_t color) {
  for (int i = 0; i < NUM_LEDS; i++) {
    strip.setPixelColor(i, color);
  }
  strip.show();
  delay(100);
}

uint32_t Wheel(byte WheelPos) {
  WheelPos = 255 - WheelPos;
  if (WheelPos < 85) return strip.Color(255 - WheelPos * 3, 0, WheelPos * 3);
  if (WheelPos < 170) {
    WheelPos -= 85;
    return strip.Color(0, WheelPos * 3, 255 - WheelPos * 3);
  }
  WheelPos -= 170;
  return strip.Color(WheelPos * 3, 255 - WheelPos * 3, 0);
}
