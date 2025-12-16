CREATE TABLE IF NOT EXISTS `FoA_DA` (
  `DaId` INT NOT NULL AUTO_INCREMENT,
  `Titel` VARCHAR(100) NOT NULL,
  `Schueler` VARCHAR(200) NOT NULL,
  PRIMARY KEY (`DaId`)
);

# +Funktionen zum aufrufen
# get unused QR-Codes (noch nicht im voting system)
CREATE TABLE if NOT EXISTS `FoA_QrCodes`(
 `QrId` VARCHAR(10) NOT NULL, 
 `used` BOOLEAN DEFAULT FALSE,
 PRIMARY KEY(`QrId`)
);
#id - bekomme paul VARCHAR

#INSERT INTO FoA_DA
#VALUES (NULL, "Die Neueste DA", "Franzl"); 

#INSERT INTO FoA_QrCodes
#VALUES ("Fq2974JK", TRUE); 


CREATE TABLE if NOT EXISTS `FoA_Voting_System`(
`VotingId`INT NOT NULL AUTO_INCREMENT,
`QrId` VARCHAR(10) NOT NULL, 
`DaId` INT NOT NULL, 
PRIMARY KEY (`VotingId`), 
FOREIGN KEY (`QrId`) REFERENCES FoA_QrCodes (`QrId`) ON DELETE CASCADE,
FOREIGN KEY (`DaId`) REFERENCES FoA_DA (`DaId`) ON DELETE CASCADE

#id# 
#QrId
#daID

) ENGINE=InnoDB;