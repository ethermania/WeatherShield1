/* -----------------------------------------------------------------
    Weather Shield 1 communication sample sketch
                               www.EtherMania.com - Signorini Marco
   
   This is a sample sketch that shows how to communicate to the
   Weather Shield and an Arduino 2009 board and to publish 
   received into a web page.
   Web client part is based on example provided by the Arduino IDE.
  ------------------------------------------------------------------- */
  
#include "weathershield1_weatherdata.h"
  
WeatherData::WeatherData() 
{ 
	bReady = false;
}

