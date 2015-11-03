from urllib.request import urlopen
from urllib.parse import quote
import io
from os import path, system
from sys import exit
import json

base_url = ['http://eu.battle.net/api/wow/character/',
             "<REALM>", "/", "<CHARACTER", "?fields=progression"]
main_realm = "Ravencrest"

def check_if_file_exist(filename):
    return path.isfile(filename)

class Boss:
    def __init__(self, name):
        self.name = name
        self.normal_kills = 0
        self.heroic_kills = 0

class Character:
    def __init__(self, realm, name):
        self.realm = realm
        self.name = name
        self.bosses = {}
        self.achis = 0

class Mauntruns:
    def __init__(self):
        self.boss_ids = []
        self.chars = []
        self.characters = []

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

    def read_characters(self):
        filename = "chars.txt"
        self.chars = self.read_file(filename)

    def read_bosses(self):
        filename = "bosses.txt"
        self.boss_ids = self.read_file(filename)

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

    def read_api(self, url):
        response = urlopen(url)
        html = response.read()
        lines = html.decode(encoding="UTF-8")
        result = json.loads(lines)
        self.process_json(result)

    def process_json(self, json):
        self.characters[-1].achis = json['achievementPoints']
       
        raids = json['progression']['raids']
        for raid in raids:
            for boss in raid['bosses']:
                 for boss_id in self.boss_ids:
                    if(boss['id'] == int(boss_id)):
                        self.process_kill(boss)

    def process_kill(self, progressBoss):
        boss = Boss(progressBoss['name'])

        boss.normal_kills = int(progressBoss['normalKills'])
        if('heroicKills' in progressBoss):
            boss.heroic_kills = progressBoss['heroicKills']
       
        self.characters[-1].bosses[boss.name] = boss

    def check_if_same_account(self):
        achis = self.characters[0].achis
        for char in self.characters:
            if(char.achis != achis):
                print("There seems to be multiple accounts. Do you still want to continue?")
                answer = input("Y/N: ")
                if(answer == 'N' or answer == 'n'):
                    exit()

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

    def get_rng(self, kills):
        drop_rate = 0.01
        rng = (1-(1 - drop_rate)**(kills.normal_kills + kills.heroic_kills))*100
        return rng

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

    def print_to_file(self, lines, filename):    
        with open(filename, 'w') as file:
            for line in lines:
                file.write(line + "\n")


system("title " + "maunts - Console Application")
maunts = Mauntruns()
maunts.run()

input("Press Enter to continue...")