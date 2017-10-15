#define SERIAL_BAUD_RATE 256000
#define READ_NEXT_WAIT 5
#define BURST_WRITE_BUFFER_SIZE 512
#define SS_N0_PIN 2
#define SS_N1_PIN 3
#define RST_N_PIN 19

#include <SPI.h>

byte device_select = 0x00;
byte burst_write_buffer[BURST_WRITE_BUFFER_SIZE];

void setup() {
  Serial.begin(SERIAL_BAUD_RATE);
  while (!Serial) ;
  pinMode (RST_N_PIN, OUTPUT);
  pinMode (SS_N0_PIN, OUTPUT);
  pinMode (SS_N1_PIN, OUTPUT);
  digitalWrite (RST_N_PIN, HIGH);
  unselect_ss();

  SPI.begin();
  SPI.beginTransaction(SPISettings(8000000, MSBFIRST, SPI_MODE0));
}

void loop() {
  command();
  delayMicroseconds(5);
}

byte serial_read_next() {
  while (!Serial.available())
    delayMicroseconds(READ_NEXT_WAIT);
  
  return (byte)Serial.read();
}

word serial_read_next16() {
  while (Serial.available() < 2)
    delayMicroseconds(READ_NEXT_WAIT);
  
  //return (word)((Serial.read() & 0xff) << 8 | (Serial.read() & 0xff));
  return (word)((Serial.read() & 0xff) | (Serial.read() & 0xff) << 8);
}

void select_ss() {
  digitalWrite(SS_N0_PIN, (device_select & 0x01) ? LOW : HIGH);
  digitalWrite(SS_N1_PIN, (device_select & 0x02) ? LOW : HIGH);
}

void select_ss_exclusively(byte select) {
  unselect_ss();
  
  if (select & 0x01) {
    digitalWrite(SS_N0_PIN, LOW);
    return;
  }
  
  if (select & 0x02) {
    digitalWrite(SS_N1_PIN, LOW);
    return;
  }
}

void unselect_ss() {
  digitalWrite(SS_N0_PIN, HIGH);
  digitalWrite(SS_N1_PIN, HIGH);
}

void spi_write() {
  byte address = serial_read_next() & 0x7f;
  byte data = serial_read_next();
  select_ss();
  SPI.transfer(address);
  SPI.transfer(data);
  unselect_ss();
}

void spi_burst_write() {
  int size_val = (int)serial_read_next16();
  byte address = serial_read_next() & 0x7f;

  if (size_val > BURST_WRITE_BUFFER_SIZE)
    size_val = BURST_WRITE_BUFFER_SIZE;

  for (int i = 0; i < size_val; i++) {
    burst_write_buffer[i] = serial_read_next();
  }
  
  select_ss();
  SPI.transfer(address);
  SPI.transfer(burst_write_buffer, size_val);
  unselect_ss();
}

byte spi_read() {
  byte device = serial_read_next();
  byte address = serial_read_next();
  byte read_buffer[2];

  read_buffer[0] = address | 0x80;
  select_ss_exclusively(device);
  SPI.transfer(read_buffer, 2);
  unselect_ss();

  return read_buffer[1];
}

void reset_hardware() {
  digitalWrite (RST_N_PIN, HIGH);
  digitalWrite (RST_N_PIN, LOW);
  delayMicroseconds(100);
  digitalWrite (RST_N_PIN, HIGH);
}

void command() {
  if (Serial.available()) {
    switch ((byte)Serial.read()) {
      case 0x00:      // Write
        spi_write();
        break;

      case 0x01:      // BurstWrite
        spi_burst_write();
        break;

      case 0x20:      // Read
        Serial.write(spi_read());
        break;
      
      case 0x40:      // Select
        device_select = serial_read_next();
        break;

      case 0xfe:      // HardReset
        reset_hardware();
        break;

      case 0xff:      // Version
        Serial.write("V1YMF825");
        break;
    }
  }
}

