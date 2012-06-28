/**

  WeatherShield server uploading data to COSM.COM
  -----------------------------------------------

  First you need an Ethermania WeatherShield and its libraries.
  You'll need a COSM api-key and a feed number to upload data to.
  You should also define the names of the time-series you are uploading to.  
  Last but not least, check the LED pin so you have a visual feedback
  of whether it's working or not (count the blinks).

  Example feeds can be found at:
  - https://cosm.com/feeds/64810

  You can get a WeatherShield from:
  - http://www.ethermania.com/shop/index.php?main_page=product_info&cPath=91_104&products_id=612&language=en

  - (c) 2012 Lenz Emilitri
  Released to the public domain.

  Created on an Arduino Ethernet with IDE 1.0.1
*/


#include <SPI.h>
#include <Ethernet.h>
#include <WeatherShield1.h>

#define APIKEY            "-------" // your cosm api key
#define FEEDID            64810 // your feed ID
#define USERAGENT         "Ethermania WeatherShield" // user agent is the project name
#define POST_EVERY        15  // how much do we pause between an update and the following one
#define PIN_LED           9   // My Arduino Ethernet has its embedded LED on line 9.
#define SERIES_PRESSURE   "Pressure"
#define SERIES_HUMIDITY   "Humidity"
#define SERIES_TEMP       "Temperature"
#define WSBUFFERLENGTH    4

#define BLINKS_OK         2  // All went well
#define BLINKS_ERR_HTTP   3  // Error posting to cosm.com
#define BLINKS_ERR_WS     5  // Error initializing the weather shield
#define BLINKS_ERR_ETH    6  // Error starting Ethernet

char server[] = "api.cosm.com";
byte mac[] = { 
  0xDE, 0xAD, 0xBE, 0xEF, 0xFE, 0xED};


// These are the clients we use
EthernetClient client;
WeatherShield1 weatherShield;

// this buffer is used to receive data from the weather shield
unsigned char ucBuffer[WSBUFFERLENGTH];


// ---------------------------------------------------------
// Initialize our baby
// ---------------------------------------------------------
void setup() {
  // start serial port:
  Serial.begin(9600);

  // turn off Arduino's LED
  pinMode(PIN_LED, OUTPUT);      
  digitalWrite( PIN_LED, LOW );


  // check that the weather shield is working
  // and set the sample time to 1s
  if (!weatherShield.sendCommand(CMD_ECHO_PAR, 100, ucBuffer)) {
    blinkForever( "Shield error", BLINKS_ERR_WS );
  }

  weatherShield.sendCommand(CMD_SET_SAMPLETIME, 1, ucBuffer);
  unsigned long startMillis = millis();  


  // start the Ethernet connection:
  if (Ethernet.begin(mac) == 0) {
    blinkForever("Failed to configure Ethernet using DHCP", BLINKS_ERR_ETH );
  }

  // wait until the ring buffer is all filled
  // so we can get averaged values that make sense
  while (( millis() - startMillis ) < 9000 ) {
    Serial.println( "Waiting for averaging buffer to be filled" );
    delay(1000);      
  }

}

/**
  We read data out of the weather shield and try posting it to cosm.
  Whether we succeed or not, we blink our led after each cycle so we
  can know if it worked or not.
*/
void loop() {

  String csvData = "";
  csvData += getCsv( SERIES_TEMP,     CMD_GETTEMP_C_AVG );
  csvData += getCsv( SERIES_HUMIDITY, CMD_GETHUM_AVG    );
  csvData += getCsv( SERIES_PRESSURE, CMD_GETPRESS_AVG  );

  if ( sendFeed( csvData ) ) {
    blink( BLINKS_OK, POST_EVERY * 1000 );
  } 
  else {
    blink( BLINKS_ERR_HTTP, POST_EVERY * 1000 );
  }
}

/**
  Print an error message and blinks the led a given number of times.
  It basically goes on forever for non-recoverable errors.
*/

void blinkForever( String msg, int nTimes ) {
  for ( ;; ) {
    Serial.print( "ERROR: " );
    Serial.println( msg );
    blink( nTimes, 5000 );
  }
}

/**
  Blinks a led a given number of times, taking all wait time.
  So if you e.g. set nTimes to 2 and wait to 10000,
  it goes "blink-blink" then pauses a second for no less
  than 10000ms. 
*/

void blink( int nTimes, int wait ) {

  unsigned long currentMillis = millis();  

  while (( millis() - currentMillis ) < wait){

    for ( int i = 0; i < nTimes; i++ ) {
      digitalWrite(PIN_LED, HIGH);
      delay(150);
      digitalWrite(PIN_LED, LOW);
      delay(250);      
    }  

    delay( 700 );
  }
}

/**
  Creates a CSV string made out of a label (the label of the time-series
  in your cosm.com feed) and a numeric value read out of the weather shield
  with the command you passed.
*/

String getCsv( String label, int command ) {
  char temp[30];

  if (weatherShield.sendCommand( command, 0, ucBuffer)) {
    weatherShield.decodeFloatAsString( ucBuffer, temp );
  }

  String s = label;
  s += ",";
  s += temp;
  s += "\n";

  return s;
}


/**
  Sends our feeds to cosm.com.
  Checks to see that we actually get a response of 200.
  If all goes well returns 0 else 1.
*/

int sendFeed( String data ) {
  Serial.println( data );

  int retVal = 0;

  if (client.connect(server, 80)) {
    // send the HTTP PUT request:
    client.print("PUT /v2/feeds/");
    client.print(FEEDID);
    client.println(".csv HTTP/1.1");
    client.println("Host: api.cosm.com");
    client.print("X-ApiKey: ");
    client.println(APIKEY);
    client.print("User-Agent: ");
    client.println(USERAGENT);
    client.print("Content-Length: ");
    client.println(data.length());

    // last pieces of the HTTP PUT request:
    client.println("Content-Type: text/csv");
    client.println("Connection: close");
    client.println();

    // here's the actual content of the PUT request:
    client.println(data);

    if ( client.find( "HTTP/1.1 200 ") ) {
      Serial.println( "Request OK" );
      retVal = 1;
    } 
    else {
      Serial.println( "ERR: No Http status 200 OK" );
    }

  } 
  else {
    Serial.println( "Could not connect to api.cosm.com" );
  }

  client.stop();
  client.flush();
  return retVal;

}







