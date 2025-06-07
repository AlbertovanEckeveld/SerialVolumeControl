void setup() {
  Serial.begin(9600);
  Serial.println("Start SerialVolumeController app");
}

void loop() {
  Serial.println("Slider0: " + String(analogRead(A0)));
  Serial.println("Slider1: " + String(analogRead(A1)));
  Serial.println("Slider2: " + String(analogRead(A2)));
  Serial.println("Slider3: " + String(analogRead(A3)));
  Serial.println("Slider4: " + String(analogRead(A4)));

}
