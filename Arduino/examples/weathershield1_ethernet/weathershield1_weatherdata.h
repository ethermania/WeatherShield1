/* -----------------------------------------------------------------
    Weather Shield 1 communication sample sketch
                               www.EtherMania.com - Signorini Marco
   
   This is a sample sketch that shows how to communicate to the
   Weather Shield and an Arduino 2009 board and to publish 
   received into a web page.
   Web client part is based on example provided by the Arduino IDE.
  ------------------------------------------------------------------- */
  
  
#ifndef _WEATHERDATA_H_
#define _WEATHERDATA_H_

/* This is a simple container we will use to
store temporary values read from the WeatherShield1 
to be written to the ethernet shield */
class WeatherData {

  public:
    WeatherData();
  
  public:
    bool bReady;
    float fTemperature;
    unsigned short shTemperature;
    float fPressure;
    unsigned short shPressure;
    float fHumidity;
    unsigned short shHumidity;
};

#endif /* _WEATHERDATA_H_ */

