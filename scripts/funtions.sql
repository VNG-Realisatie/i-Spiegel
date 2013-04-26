CREATE OR REPLACE FUNCTION HTMLUNESCAPE
(
  INCOMING IN VARCHAR2  
) RETURN VARCHAR2 AS 
BEGIN
  RETURN
    REPLACE(  
        REPLACE(  
          REPLACE(  
            REPLACE(  
              REPLACE(  
                REPLACE(  
                  REPLACE(  
                    REPLACE(
                      REPLACE(INCOMING, '&#0250;', 'ú')
                    ,'&#0226;', 'â')
                  ,'&#0234;', 'ê')
                ,'&#0251;', 'û')
              ,'&#0244;', 'ô')
            ,'&#0233;', 'é')
          ,'&#0235;', 'ë')    
        ,'&#0224;', 'à')        
      ,'&#0239;', 'ï');
END HTMLUNESCAPE;