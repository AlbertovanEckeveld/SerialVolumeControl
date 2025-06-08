int prevValues[5] = {0, 0, 0, 0, 0}; 
const int threshold = 2; 

void setup() {
  Serial.begin(9600);
}

void loop() {
  sendSliderData();
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
