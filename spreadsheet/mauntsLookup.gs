/**
 * MauntsLookup - Reads the wow API and gets the amount of kills for given boss.
 * Created by Fredrik G 2015/11/01.
*/

/**
 * Reads mounts for a given player and returns result.
 * The bosses to lookup are based on column1 in the spreadsheet.
 * @param {String} name 
 * @param {String} realm
 * @return {Array} result
 */
function readMaunts(name, realm, bosses) {
  //Set up boss IDs
  var bossIds = [];
  for(var i = 0; i < bosses[0].length; i++){
    if(bosses[0][i] != "" && bosses[0][i].indexOf("#") != -1){
      bossIds.push(bosses[0][i].split("#")[0]); 
    }
  }

  var url = "http://eu.battle.net/api/wow/character/"+realm+"/"+name+"?fields=progression";
  var characterString = UrlFetchApp.fetch(url).getContentText();

  var characterProgress = JSON.parse(characterString);

  //Calculate boss kills for each boss ID.
  var kills = [];
  for(i = 0; i < bossIds.length; i++){
    for(j = 0; j < characterProgress.progression.raids.length; j++){
      for(k = 0; k < characterProgress.progression.raids[j].bosses.length; k++){
        var boss = characterProgress.progression.raids[j].bosses[k];
        if(bossIds[i] == boss.id){
          var normalKills = boss.normalKills;
          var heroicKills = 0;
          if(boss.heroicKills){
           var heroicKills = boss.heroicKills;
          }        
          var killString = buildKillString(normalKills, heroicKills);
          kills.push(killString);
        }
      }
    }
  }
  
  //Get a string with current date.
  var currentDate = new Date();
  var lastUpdate = (currentDate.getMonth() + 1) + "/" 
  + currentDate.getDate() + "/" 
  + currentDate.getFullYear() + " @ "  
  + currentDate.getHours() + ":"  
  + currentDate.getMinutes() + ":" 
  + currentDate.getSeconds() + " CST";
  
 var result = new Array();
 for(i = 0; i < kills.length; i++){
    result.push(kills[i]);
  }
 result.push(lastUpdate);

 return result;
}

/**
 * Gets RNG for given kill amount.
 * @param {Number} kills
 * @return {Number} rng
 */
function getRng(kills){
  var dropRate = 0.01;
  var rng = (1 - Math.pow((1 - dropRate), kills)) * 100;
  return Math.round(rng * 10) / 10
}

/**
 * Builds a kill-string based on sent kills.
 * @param {Number} normalKills
 * @param {Number} heroicKills
 * @return {String} kill-string
 */
function buildKillString(normalKills, heroicKills){
  return normalKills.toString() + "-" + heroicKills.toString() + " " + getRng(normalKills+heroicKills).toString() + "%";
}

/**
 * Gets the total number (sum) of kills based of sent kills.
 * @param {Array of Strings} kills
 * @return {String} total
 */
function getTotal(kills){
  var normalKills = 0;
  var heroicKills = 0;
  for(var i = 0; i < kills.length; i++){
    normalKills += parseInt(kills[i].toString().split("-")[0]);
    heroicKills += parseInt(kills[i].toString().split("-")[1]);
  }
  return buildKillString(normalKills, heroicKills);
}

/**
 * Checks if given character has collected given bosses.
 * @param {String} character
 * @param {String} realm
 * @param {Array of Strings} bosses
 * @return {Array} collected mounts
 */
function checkIfOwned(character, realm, bosses){
 var collectedMounts = [];
 for(var i = 0; i < bosses[0].length; i++){
    if(bosses[0][i] != "" && bosses[0][i].indexOf("#") != -1){
      var IDs = bosses[0][i].split("#");
      IDs.splice(0, 2);
      collectedMounts.push([IDs, 0]);     
    }
  }

  var characterMounts = getCharacterJSON(character, realm, "mounts");
  for(var i = 0; i < characterMounts.mounts.collected.length; i++){
    var mount = characterMounts.mounts.collected[i];
    for(var j = 0; j < collectedMounts.length; j++){
      for(var k = 0; k < collectedMounts[j][0].length; k++){
        if(mount.itemId == parseInt(collectedMounts[j][0][k])){
          collectedMounts[j][1] += 1;
        } 
      }
    }
  }      
 return collectedMounts;
}

/**
 * Reads the API and gets a character JSON-object based on given data.
 * @param {String} name
 * @param {String} realm
 * @return {Object} JSON-object
 */
function getCharacterJSON(name, realm, field){
  var url = "http://eu.battle.net/api/wow/character/"+realm+"/"+name+"?fields="+field;
  var characterString = UrlFetchApp.fetch(url).getContentText();
  return JSON.parse(characterString);  
}

/**
 * Forces cells to update by changing the value of a dummy cell. 
 */
function forceUpdate()
{
  var newValue = "dummy";
  var cell = SpreadsheetApp.getActiveSheet().getRange('A20');
  if(cell.getValue() != "" && cell.getValue() == newValue){
    var newValue = "dummy2"
  }
  cell.setValue(newValue);
}