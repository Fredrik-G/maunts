from urllib.request import urlopen
from urllib.parse import quote
import io
from os import path, system
from sys import exit
import json

#base url, pointing to the wow API.
base_url = ['http://eu.battle.net/api/wow/character/',
             "<REALM>", "/", "<CHARACTER", "?fields=progression"]
#Main real, characters without a set realm will use this one.
main_realm = "Ravencrest"

"""
Checks if a given file exists
@param filename: the filename to check for.
@return: file exist true/false
"""
def check_if_file_exist(filename):
    return path.isfile(filename)

"""
Class containing information about a boss.
"""
class Boss:
    def __init__(self, name):
        self.name = name
        self.normal_kills = 0
        self.heroic_kills = 0

"""
Class containing information about a character.
"""
class Character:
    def __init__(self, realm, name):
        self.realm = realm
        self.name = name
        self.bosses = {}
        self.achis = 0

"""
Main/base class containing maunts-related methods
lists of characters&bosses.
"""
class Mauntruns:
    def __init__(self):
        self.boss_ids = []
        self.chars = []
        self.characters = []

    """
    Runs the maunt lookup.
    """
    def run(self):
        try:
            self.read_characters()
            self.read_bosses()
            self.lookup_player()
            self.check_if_same_account()
            self.print_result()
        except FileNotFoundError as error:
            print(error)
            input("Press Enter to continue...")

    """
    Reads characters to the Character-list.
    """
    def read_characters(self):
        filename = "chars.txt"
        self.chars = self.read_file(filename)

    """
    Reads bosses to the Boss-list.
    """
    def read_bosses(self):
        filename = "bosses.txt"
        self.boss_ids = self.read_file(filename)

    """
    Reads a given file and return its lines.
    Ignores lines starting with "#" and everything after "#" mid-line.
    @param filename: the file to read.
    @returns: list of lines.
    """
    def read_file(self, filename):
        if(not check_if_file_exist(filename)):
            raise FileNotFoundError("Filen " + filename + " hittades ej.")

        lines = []        
        with open(filename, 'r') as file:
            for line in file:
                if(line.startswith("realm=")):
                   main_realm = line.split("=")[1].strip()
                elif(not line.startswith("#") and not line == "\n"):
                    if("#" in line):
                        lines.append(line.split('#')[0])
                    else:
                        lines.append(line.strip())

        return lines

    """
    Looks up every player in the Character-list.
    """
    def lookup_player(self):
        for character in self.chars:
            if(character.find('(') != -1):
                splitted = character.split(' ')
                character = splitted[0]
                realm = splitted[1].split('(')[1].split(')')[0]
            else:
                realm = main_realm
          
            char = Character(realm, character)
            self.characters.append(char)
            base_url[1] = char.realm
            base_url[3] = quote(char.name)

            url = ""
            for line in base_url:
                url += line

            self.read_api(url)

    """
    Reads the wow API and loads the result as JSON.
    """
    def read_api(self, url):
        response = urlopen(url)
        html = response.read()
        lines = html.decode(encoding="UTF-8")
        result = json.loads(lines)
        self.process_json(result)

    """
    Processes a JSON "object" (dict).
    Iterates it and checks for boss kills.
    """
    def process_json(self, json):
        self.characters[-1].achis = json['achievementPoints']
       
        raids = json['progression']['raids']
        for raid in raids:
            for boss in raid['bosses']:
                 for boss_id in self.boss_ids:
                    if(boss['id'] == int(boss_id)):
                        self.process_kill(boss)

    """
    Processes a boss and adds its kills to the right character.
    """
    def process_kill(self, progressBoss):
        boss = Boss(progressBoss['name'])

        boss.normal_kills = int(progressBoss['normalKills'])
        if('heroicKills' in progressBoss):
            boss.heroic_kills = progressBoss['heroicKills']
       
        self.characters[-1].bosses[boss.name] = boss

    """
    Checks if all characters in the Character-list is of the same account
    and warns the user if it appears to be different accounts.
    Determine this by comparing each character's achievement points.
    """
    def check_if_same_account(self):
        achis = self.characters[0].achis
        for char in self.characters:
            if(char.achis != achis):
                print("There seems to be multiple accounts. Do you still want to continue?")
                answer = input("Y/N: ")
                if(answer == 'N' or answer == 'n'):
                    exit()
    """
    Gets the total number of kills.
    @return: dict containing the total number of kills.
    """
    def get_total_kills(self):
        total_kills = {}
        for character in self.characters:
            for key, kills in character.bosses.items():
                boss = Boss(key)
                boss.normal_kills = kills.normal_kills
                boss.heroic_kills = kills.heroic_kills
                if (key in total_kills):
                    total_kills[key].normal_kills += boss.normal_kills
                    total_kills[key].heroic_kills += boss.heroic_kills
                else:
                    total_kills[key] = boss
        return total_kills

    """ 
    Gets the procentage of people who received a mount based on character's kill amount.
    Formula is 1-(1-0.01)^total_kills, where 0.01 (1%) is the default drop rate.
    (RNG = Random Number Generator, aka this checks how lucky you are.)
    @return: rng procentage.
    """
    def get_rng(self, kills):
        drop_rate = 0.01
        rng = (1-(1 - drop_rate)**(kills.normal_kills + kills.heroic_kills))*100
        return rng

    """
    Prints the result of the mount-lookup.
    Also saves result as a text file.
    """
    def print_result(self):
        print("Kills for player %s" % self.characters[0].name)
        total_kills = self.get_total_kills()
        lines = []
        for boss in sorted(total_kills):
            kills = total_kills[boss]
            rng = self.get_rng(kills)
            lines.append("%s total kills: (%d-%d) - %% of people with maunt: %.1f%%"
                         % (boss, kills.normal_kills, kills.heroic_kills, rng))
            print(lines[-1])

        filename = self.characters[0].name + ".txt"
        self.print_to_file(lines, filename)

    """
    Prints/writes given lines to given file.
    @param lines: the lines to print
    @param filename: the filename to write to.
    """
    def print_to_file(self, lines, filename):    
        with open(filename, 'w') as file:
            for line in lines:
                file.write(line + "\n")


system("title " + "maunts - Console Application")
maunts = Mauntruns()
maunts.run()

input("Press Enter to continue...")