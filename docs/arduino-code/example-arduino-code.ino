void setup() {
  Serial.begin(9600);
  Serial.println("Start SerialVolumeController app");
}

void loop() {
  sendSliderData();
  
}

void getThema() {}

void sendSliderData() {
  Serial.println("slider0: " + String(analogRead(A0)));
  Serial.println("slider1: " + String(analogRead(A1)));
  Serial.println("slider2: " + String(analogRead(A2)));
  Serial.println("slider3: " + String(analogRead(A3)));
  Serial.println("slider4: " + String(analogRead(A4)));
}
