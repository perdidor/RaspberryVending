#define  HFUSE  0x5F
#define  LFUSE  0x62

#define I2C_SLAVE_ADDRESS 0x41
#define DHTPIN 4
#define DHTTYPE DHT22

#include <TinyWireS.h>
#include <OneWire.h>
#include "DHT.h"

#ifndef TWI_RX_BUFFER_SIZE
#define TWI_RX_BUFFER_SIZE ( 16 )
#endif

struct AddressAndTemp {
  byte Address[8];
  byte Temp[4];
};

struct TempAndHumidity {
  byte Temp[4];
  byte Humidity[4];
};

AddressAndTemp ATData[2];
TempAndHumidity AmbientTempAndHumidity;

union floatAsBytes {
    float fval;
    byte bval[4];
};

floatAsBytes tmp;

volatile uint8_t i2c_regs[] =
{
    0xDE, 
    0xAD, 
    0xBE, 
    0xEF, 
};

volatile byte reg_position;
const byte reg_size = sizeof(i2c_regs);

DHT dht(DHTPIN, DHTTYPE);

OneWire  ds(3);

void requestEvent()
{
  for (int i = 0; i < 2; i++) {    
    for (int a = 0; a < 8; a++) {          
      TinyWireS.send(ATData[i].Address[a]);
    }   
    for (int b = 0; b < 4; b++) {          
      TinyWireS.send(ATData[i].Temp[b]);
    }    
  }
  for (int c = 0; c < 4; c++) {          
      TinyWireS.send(AmbientTempAndHumidity.Temp[c]);
  }
  for (int d = 0; d < 4; d++) {          
    TinyWireS.send(AmbientTempAndHumidity.Humidity[d]);
  }
}

void setup()
{
  delay(2000);
  dht.begin();
  TinyWireS.begin(I2C_SLAVE_ADDRESS);
  TinyWireS.onRequest(requestEvent);
}

void GetOneWireTemp()
{
  byte i;
  byte present = 0;
  byte type_s;
  byte data[12];
  byte addr[8];
  float celsius;
  if ( !ds.search(addr)) {
    ds.reset_search();
    tws_delay(250);
    return;
  }
  if (OneWire::crc8(addr, 7) != addr[7]) {
      return;
  }
  switch (addr[0]) {
    case 0x10:
      type_s = 1;
      break;
    case 0x28:
      type_s = 0;
      break;
    case 0x22:
      type_s = 0;
      break;
    default:
      return;
  } 
  ds.reset();
  ds.select(addr);
  ds.write(0x44, 1);
  tws_delay(1000);
  present = ds.reset();
  ds.select(addr);    
  ds.write(0xBE);         // Read Scratchpad
  for ( i = 0; i < 9; i++) {           // we need 9 bytes
    data[i] = ds.read();
  }
  int16_t raw = (data[1] << 8) | data[0];
  if (type_s) {
    raw = raw << 3; // 9 bit resolution default
    if (data[7] == 0x10) {
      // "count remain" gives full 12 bit resolution
      raw = (raw & 0xFFF0) + 12 - data[6];
    }
  } else {
    byte cfg = (data[4] & 0x60);
    // at lower res, the low bits are undefined, so let's zero them
    if (cfg == 0x00) raw = raw & ~7;  // 9 bit resolution, 93.75 ms
    else if (cfg == 0x20) raw = raw & ~3; // 10 bit res, 187.5 ms
    else if (cfg == 0x40) raw = raw & ~1; // 11 bit res, 375 ms
    //// default is 12 bit resolution, 750 ms conversion time
  }
  tmp.fval = (float)raw / 16.0;
  memcpy(ATData[0].Address, addr, 8);
  memcpy(ATData[0].Temp, tmp.bval, 4);
  if ( !ds.search(addr)) {
    ds.reset_search();
    tws_delay(250);
    return;
  }
  if (OneWire::crc8(addr, 7) != addr[7]) {
      return;
  }
  switch (addr[0]) {
    case 0x10:
      type_s = 1;
      break;
    case 0x28:
      type_s = 0;
      break;
    case 0x22:
      type_s = 0;
      break;
    default:
      return;
  } 
  ds.reset();
  ds.select(addr);
  ds.write(0x44, 1);
  tws_delay(1000);
  present = ds.reset();
  ds.select(addr);    
  ds.write(0xBE);         // Read Scratchpad
  for ( i = 0; i < 9; i++) {           // we need 9 bytes
    data[i] = ds.read();
  }
  raw = (data[1] << 8) | data[0];
  if (type_s) {
    raw = raw << 3; // 9 bit resolution default
    if (data[7] == 0x10) {
      // "count remain" gives full 12 bit resolution
      raw = (raw & 0xFFF0) + 12 - data[6];
    }
  } else {
    byte cfg = (data[4] & 0x60);
    // at lower res, the low bits are undefined, so let's zero them
    if (cfg == 0x00) raw = raw & ~7;  // 9 bit resolution, 93.75 ms
    else if (cfg == 0x20) raw = raw & ~3; // 10 bit res, 187.5 ms
    else if (cfg == 0x40) raw = raw & ~1; // 11 bit res, 375 ms
    //// default is 12 bit resolution, 750 ms conversion time
  }
  tmp.fval = (float)raw / 16.0;
  memcpy(ATData[1].Address, addr, 8);
  memcpy(ATData[1].Temp, tmp.bval, 4);
}


void loop()
{
  TinyWireS_stop_check();
  GetOneWireTemp();
  tmp.fval = dht.readTemperature();
  memcpy(AmbientTempAndHumidity.Temp, tmp.bval, 4);
  tmp.fval = dht.readHumidity();
  memcpy(AmbientTempAndHumidity.Humidity, tmp.bval, 4);
}
