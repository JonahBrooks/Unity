#include <iostream>
#include <fstream>
#include <vector>
#include <string>
#include <sstream>
#include <queue>
#include <stdlib.h>
#include <time.h>
#include <cmath>
#include <algorithm>

using namespace std;

// Rules of the game:
//  The game map is filled with the starting positions and directions of all objects
//  The game is played by moving your deer up, down, left, or right
//  When the deer moves, all other characters move as well
//  A wolf will eat the deer if they are in the same tile
//  A hunter will shoot the deer if it is in his line of travel
//  The player must get his deer to the exit without being eaten



// FMap legend:
//  'D ' is the deer
//  'B ' is a block
//  'W*' is a wolf going in * direction ( * = {u,d,l,r} )
//  'H*' is a hunter going in * direction
//  'Ex' is the exit

// Map legend:
//  'D' is the deer
//  'B' is a block
//  '2' is a wolf going up, 'S' is a wolf going down, 'E' is a wolf going right, 'Q' is a wolf going left
//  'Y' is a hunter going up, 'N' is a hunter going down, 'J' is a hunter going right, 'G' is a hunter going left
//  'X' is the exit


// Class Unit:
//  Used to store information on a single unit / square on grid

class Unit
{
  public:
    static int total_Units;
    static Unit space;
    static Unit edge;
    static Unit exit;
    static Unit deer;
    int x;
    int y;
    char c;
    int id;
    Unit();
    Unit(char type);
    Unit(char type, int i);
} ;

int Unit::total_Units = 0;

Unit Unit::space = Unit(' ',-1);
Unit Unit::edge = Unit('B',-2);
Unit Unit::exit = Unit('X',-3);
Unit Unit::deer = Unit('D',-4);

Unit::Unit()
{
  c = ' ';
  id = -1;
}

Unit::Unit(char type)
{
  c = type;
  id = total_Units++;
}

Unit::Unit(char type, int i)
{
  c = type;
  id = i;
}


// Class Map:
//  Used to store the game map and supporting information
class Map
{

  public:
    static float ideal;
    int line_length;
    int deeri;
    int deerx;
    int deery;
    int exiti;
    int exitx;
    int exity;
    float rating;
    vector<Unit> map;
    Map();
    Map(int size, int line_size);
    Map(string file_name); 
    Map(const Map& old_map);
    int size();
    int convert_to_index(int x, int y);
    void convert_from_index(int * x, int * y, int i);
    void regenerate_map();
    void advance_map();
    void simplify_map();
    Unit get_element(int x, int y);
    void print_map();
    Unit update_map(int x,int y);
    void save_map(string name);
    string toString();
    bool operator<(const Map &rhs) const { return abs(rating-ideal) < abs(rhs.rating-ideal); }
    ostream& operator<< (ostream& out, const Map map) { return out << map.toString(); }
};

float Map::ideal = 30;

Map::Map()
{
  map = vector<Unit>(16*9);
  line_length = 16;
  regenerate_map();
  rating = 101;
}

Map::Map(int size, int line_size)
{
  map = vector<Unit>(size);
  line_length = line_size;
  regenerate_map();
  rating = 101;
}
    
Map::Map(string file_name)
{
  
  char one,two,three,four;
  string line;
  vector<char> raw_map;

  ifstream fmap;
  fmap.open(file_name.c_str());
  while(getline(fmap,line))
  {
    line_length = line.length()/4;
    for(int i = 0; i < line.length(); i++)
    {
      raw_map.push_back(line[i]);
    }    
  }
  fmap.close();

  for(int i = 0; i < raw_map.size(); i+=4)
  {
    one = raw_map[i];
    two = raw_map[i+1];
    three = raw_map[i+2];
    four = raw_map[i+3];

    // Validate input
    if(one == '[' && four == ']')
    {
      if(two == ' ')
      {
        // Empty space
        map.push_back(Unit::space);
      } else if(two == 'D')
      {
        // Deer
        map.push_back(Unit::deer);
        deeri = map.size()-1;;
        convert_from_index(&deerx, &deery, deeri);
      } else if (two == 'E')
      {
        // Exit
        map.push_back(Unit::exit); 
        exiti = map.size()-1;
        convert_from_index(&exitx, &exity, exiti);
      } else if (two == 'B')
      {
        map.push_back(Unit::edge);
      } else if (two == 'W')
      {
        // Wolf
        if (three == 'l')
        {
          map.push_back(* new Unit('Q'));
        } else if (three == 'r')
        {
          map.push_back(* new Unit('E'));
        } else if (three == 'u')
        {
          map.push_back(* new Unit('2'));
        } else if (three == 'd')
        {
          map.push_back(* new Unit('S'));
        }
      } else if (two == 'H')
      {
        // Hunter
        if (three == 'l')
        {
          map.push_back(* new Unit('G'));
        } else if (three == 'r')
        {
          map.push_back(* new Unit('J'));
        } else if (three == 'u')
        {
          map.push_back(* new Unit('Y'));
        } else if (three == 'd')
        {
          map.push_back(* new Unit('N'));
        }
      }
    }
  } 
  rating = 101;
} 
    

Map::Map(const Map& old_map)
{
    line_length = old_map.line_length; 
    deeri = old_map.deeri; 
    deerx = old_map.deerx;
    deery = old_map.deery;
    exiti = old_map.exiti;
    exitx = old_map.exitx;
    exity = old_map.exity;
    map = old_map.map;
    rating = old_map.rating;
}

int Map::size()
{
  return map.size();
}

int Map::convert_to_index(int x, int y)
{

  int i = 0;
  i = x;
  i += y*line_length;
  return i;
}

void Map::convert_from_index(int * x, int * y, int i)
{
  *x = i%(line_length);
  *y = i/(line_length);
}

void Map::regenerate_map()
{

  int deer_index = 0;
  int exit_index = map.size()-1;

  deer_index = rand() % map.size();
  exit_index = rand() % map.size();

  while(deer_index == exit_index && map.size() > 1)
  {
    exit_index = rand() % map.size();
  }

  for(int i = 0; i < map.size(); i++)
  {
    if(i == deer_index)
    {
      map[i] = Unit::deer;
      deeri = i;  
      convert_from_index(&deerx, &deery, deeri);
    }
    else if(i == exit_index)
    {
      map[i] = Unit::exit;
      exiti = i;  
      convert_from_index(&exitx, &exity, exiti);
    }
    else
    {
      map[i] = Unit::space;
    }
  }
  rating = 101;
}

void Map::advance_map()
{
  int type = rand() % 3; // Pick Block, Wolf, or Hunter
  int direction = rand() % 4; // Pick a direction for the wolf or hunter to face
  int index = rand() % map.size();  

  // Don't overwrite deer or exit, okay to overwrite anything else
  while((map[index].c == 'D' || map[index].c == 'X') && map.size() > 2)
  {
    index = rand() % map.size();
  }

  if(type == 0)
  {
      map[index].c = 'B';
  }
  else if (type == 1)
  {
    switch(direction)
    {
      case 0:
        map[index].c = '2';
        break;
      case 1:
        map[index].c = 'Q';
        break;
      case 2:
        map[index].c = 'S';
        break;
      case 3:
        map[index].c = 'E';
        break;
    }
  }
  else
  {
    switch(direction)
    {
      case 0:
        map[index].c = 'Y';
        break;
      case 1:
        map[index].c = 'G';
        break;
      case 2:
        map[index].c = 'N';
        break;
      case 3:
        map[index].c = 'J';
        break;
    }
  }

}

void Map::simplify_map()
{
  vector<int> indexes;

  // Gather indexes of objects
  for(int i = 0; i < map.size(); i++)
  {
    if(!(map[i].c == ' ' || map[i].c == 'D' || map[i].c == 'X'))
    {
      indexes.push_back(i);
    }
  }

  if(indexes.size() > 0)
  {
    random_shuffle(indexes.begin(), indexes.end());
    map[indexes[0]] = Unit::space;
  }

  

}
Unit Map::get_element(int x, int y)
{  
  int i = convert_to_index(x,y);
  if (i < 0 || i >= size() || x < 0 || x >= line_length)
  {
    return Unit::edge;
  }
  else
  {
    return map[i];
  }
}



void Map::print_map()
{
  for(int i = 0; i < size(); i++)
  {
    cout << map[i].c;
    if ((i+1)%(line_length) == 0)
    {
      cout << endl;
    }
  }

}


// Returns overlap (the character that was replaced by D after updates)
Unit Map::update_map(int x,int y)
{
  Unit overlap = Unit::space;;
  
  int vision_index = 0;
  int old_x = deerx;
  int old_y = deery;

  char next_tile;
  Unit u(' ');

  queue<Unit> q; // For the first pass of movement
  queue<Unit> q2;// For the second pass of movement
  queue<Unit> q3;// For hunter vision lines

  // Clear Deer from old position
  map[convert_to_index(old_x,old_y)].c = ' '; 

  // Move wolves and hunters 
    // Fill queue
  for(int i = 0; i < map.size(); i++)
  {
    convert_from_index((&map[i].x), (&map[i].y), i);
    if(map[i].c == 'Q' || map[i].c == 'S' || map[i].c == 'E' || map[i].c == '2')
    {
      q.push(map[i]);
    }
    if(map[i].c == 'G' || map[i].c == 'N' || map[i].c == 'J' || map[i].c == 'Y')
    {
      q.push(map[i]);
    }
    if(map[i].c == 'H') // Hunter vision line
    {
      map[i].c = ' '; // Clear hunter vision lines
      map[i].id = Unit::space.id;
    }
  }  
    // Parse queue
  while (!q.empty())
  {
    u = q.front();
    q.pop();

    switch (u.c)
    {
      case 'Q':
        next_tile = get_element(u.x-1,u.y).c;
        if(next_tile != ' ' && next_tile != 'H')
        {
          // Turn around
          q2.push(map[convert_to_index(u.x,u.y)]);
        }
        else
        {
          map[convert_to_index(u.x-1,u.y)].c = 'Q';
          map[convert_to_index(u.x-1,u.y)].id = u.id;
          map[convert_to_index(u.x,u.y)].c = ' ';
          map[convert_to_index(u.x,u.y)].id = Unit::space.id;
        }
        break;
      case 'S':
        next_tile = get_element(u.x,u.y+1).c;
        if(next_tile != ' ' && next_tile != 'H')
        {
          // Turn around
          q2.push(map[convert_to_index(u.x,u.y)]);
        }
        else
        {
          map[convert_to_index(u.x,u.y+1)].c = 'S';
          map[convert_to_index(u.x,u.y+1)].id = u.id;
          map[convert_to_index(u.x,u.y)].c = ' ';
          map[convert_to_index(u.x,u.y)].id = Unit::space.id;
        }
        break;
      case 'E':
        next_tile = get_element(u.x+1,u.y).c;
        if(next_tile != ' ' && next_tile != 'H')
        {
          // Turn around
          q2.push(map[convert_to_index(u.x,u.y)]);
        }
        else
        {
          map[convert_to_index(u.x+1,u.y)].c = 'E';
          map[convert_to_index(u.x+1,u.y)].id = u.id;
          map[convert_to_index(u.x,u.y)].c = ' ';
          map[convert_to_index(u.x,u.y)].id = Unit::space.id;
        }
        break;
      case '2':
        next_tile = get_element(u.x,u.y-1).c;
        if(next_tile != ' ' && next_tile != 'H')
        {
          // Turn around
          q2.push(map[convert_to_index(u.x,u.y)]);
        }
        else
        {
          map[convert_to_index(u.x,u.y-1)].c = '2';
          map[convert_to_index(u.x,u.y-1)].id = u.id;
          map[convert_to_index(u.x,u.y)].c = ' ';
          map[convert_to_index(u.x,u.y)].id = Unit::space.id;
        }
        break;
      case 'G':
        next_tile = get_element(u.x-1,u.y).c;
        if(next_tile != ' ' && next_tile != 'H')
        {
          // Turn around
          q2.push(map[convert_to_index(u.x,u.y)]);
        }
        else
        {
          map[convert_to_index(u.x-1,u.y)].c = 'G';
          map[convert_to_index(u.x-1,u.y)].id = u.id;
          map[convert_to_index(u.x,u.y)].c = ' ';
          map[convert_to_index(u.x,u.y)].id = Unit::space.id;
          // Prep to draw vision lines
          q3.push(map[convert_to_index(u.x-1,u.y)]);
        }
        break;
      case 'N':
        next_tile = get_element(u.x,u.y+1).c;
        if(next_tile != ' ' && next_tile != 'H')
        {
          // Turn around
          q2.push(map[convert_to_index(u.x,u.y)]);
        }
        else
        {
          map[convert_to_index(u.x,u.y+1)].c = 'N';
          map[convert_to_index(u.x,u.y+1)].id = u.id;
          map[convert_to_index(u.x,u.y)].c = ' ';
          map[convert_to_index(u.x,u.y)].id = Unit::space.id;
          // Prep to draw vision lines
          q3.push(map[convert_to_index(u.x,u.y+1)]);
        }
        break;
      case 'J':
        next_tile = get_element(u.x+1,u.y).c;
        if(next_tile != ' ' && next_tile != 'H')
        {
          // Turn around
          q2.push(map[convert_to_index(u.x,u.y)]);
        }
        else
        {
          map[convert_to_index(u.x+1,u.y)].c = 'J';
          map[convert_to_index(u.x+1,u.y)].id = u.id;
          map[convert_to_index(u.x,u.y)].c = ' ';
          map[convert_to_index(u.x,u.y)].id = Unit::space.id;
          // Prep to draw vision lines
          q3.push(map[convert_to_index(u.x+1,u.y)]);
        }
        break;
      case 'Y':
        next_tile = get_element(u.x,u.y-1).c;
        if(next_tile != ' ' && next_tile != 'H')
        {
          // Turn around
          q2.push(map[convert_to_index(u.x,u.y)]);
        }
        else
        {
          map[convert_to_index(u.x,u.y-1)].c = 'Y';
          map[convert_to_index(u.x,u.y-1)].id = u.id;
          map[convert_to_index(u.x,u.y)].c = ' ';
          map[convert_to_index(u.x,u.y)].id = Unit::space.id;
          // Prep to draw vision lines
          q3.push(map[convert_to_index(u.x,u.y-1)]);
        }
        break;
    }
  }
    // Parse queue
  while (!q2.empty())
  {
    u = q2.front();
    q2.pop();

    switch (u.c)
    {
      case 'Q':
        next_tile = get_element(u.x-1,u.y).c;
        if(next_tile != ' ' && next_tile != 'H')
        {
          // Turn around
          map[convert_to_index(u.x,u.y)].c = 'E';
        }
        else
        {
          map[convert_to_index(u.x-1,u.y)].c = 'Q';
          map[convert_to_index(u.x-1,u.y)].id = u.id;
          map[convert_to_index(u.x,u.y)].c = ' ';
          map[convert_to_index(u.x,u.y)].id = Unit::space.id;
        }
        break;
      case 'S':
        next_tile = get_element(u.x,u.y+1).c;
        if(next_tile != ' ' && next_tile != 'H')
        {
          // Turn around
          map[convert_to_index(u.x,u.y)].c = '2';
        }
        else
        {
          map[convert_to_index(u.x,u.y+1)].c = 'S';
          map[convert_to_index(u.x,u.y+1)].id = u.id;
          map[convert_to_index(u.x,u.y)].c = ' ';
          map[convert_to_index(u.x,u.y)].id = Unit::space.id;
        }
        break;
      case 'E':
        next_tile = get_element(u.x+1,u.y).c;
        if(next_tile != ' ' && next_tile != 'H')
        {
          // Turn around
          map[convert_to_index(u.x,u.y)].c = 'Q';
        }
        else
        {
          map[convert_to_index(u.x+1,u.y)].c = 'E';
          map[convert_to_index(u.x+1,u.y)].id = u.id;
          map[convert_to_index(u.x,u.y)].c = ' ';
          map[convert_to_index(u.x,u.y)].id = Unit::space.id;
        }
        break;
      case '2':
        next_tile = get_element(u.x,u.y-1).c;
        if(next_tile != ' ' && next_tile != 'H')
        {
          // Turn around
          map[convert_to_index(u.x,u.y)].c = 'S';
        }
        else
        {
          map[convert_to_index(u.x,u.y-1)].c = '2';
          map[convert_to_index(u.x,u.y-1)].id = u.id;
          map[convert_to_index(u.x,u.y)].c = ' ';
          map[convert_to_index(u.x,u.y)].id = Unit::space.id;
        }
        break;
      case 'G':
        next_tile = get_element(u.x-1,u.y).c;
        if(next_tile != ' ' && next_tile != 'H')
        {
          // Turn around
          map[convert_to_index(u.x,u.y)].c = 'J';
          // Prep to draw vision lines
          q3.push(map[convert_to_index(u.x,u.y)]);
        }
        else
        {
          map[convert_to_index(u.x-1,u.y)].c = 'G';
          map[convert_to_index(u.x-1,u.y)].id = u.id;
          map[convert_to_index(u.x,u.y)].c = ' ';
          map[convert_to_index(u.x,u.y)].id = Unit::space.id;
          // Prep to draw vision lines
          q3.push(map[convert_to_index(u.x-1,u.y)]);
        }
        break;
      case 'N':
        next_tile = get_element(u.x,u.y+1).c;
        if(next_tile != ' ' && next_tile != 'H')
        {
          // Turn around
          map[convert_to_index(u.x,u.y)].c = 'Y';
          // Prep to draw vision lines
          q3.push(map[convert_to_index(u.x,u.y)]);
        }
        else
        {
          map[convert_to_index(u.x,u.y+1)].c = 'N';
          map[convert_to_index(u.x,u.y+1)].id = u.id;
          map[convert_to_index(u.x,u.y)].c = ' ';
          map[convert_to_index(u.x,u.y)].id = Unit::space.id;
          // Prep to draw vision lines
          q3.push(map[convert_to_index(u.x,u.y+1)]);
        }
        break;
      case 'J':
        next_tile = get_element(u.x+1,u.y).c;
        if(next_tile != ' ' && next_tile != 'H')
        {
          // Turn around
          map[convert_to_index(u.x,u.y)].c = 'G';
          // Prep to draw vision lines
          q3.push(map[convert_to_index(u.x,u.y)]);
        }
        else
        {
          map[convert_to_index(u.x+1,u.y)].c = 'J';
          map[convert_to_index(u.x+1,u.y)].id = u.id;
          map[convert_to_index(u.x,u.y)].c = ' ';
          map[convert_to_index(u.x,u.y)].id = Unit::space.id;
          // Prep to draw vision lines
          q3.push(map[convert_to_index(u.x+1,u.y)]);
        }
        break;
      case 'Y':
        next_tile = get_element(u.x,u.y-1).c;
        if(next_tile != ' ' && next_tile != 'H')
        {
          // Turn around
          map[convert_to_index(u.x,u.y)].c = 'N';
          // Prep to draw vision lines
          q3.push(map[convert_to_index(u.x,u.y)]);
        }
        else
        {
          map[convert_to_index(u.x,u.y-1)].c = 'Y';
          map[convert_to_index(u.x,u.y-1)].id = u.id;
          map[convert_to_index(u.x,u.y)].c = ' ';
          map[convert_to_index(u.x,u.y)].id = Unit::space.id;
          // Prep to draw vision lines
          q3.push(map[convert_to_index(u.x,u.y-1)]);
        }
        break;
    }
  }  
    // Parse queue
  while (!q3.empty())
  {
    u = q3.front();
    q3.pop();

    switch (u.c)
    {
      case 'G':
        // Fill vision line to the left
        vision_index = 1;
        next_tile = get_element(u.x-vision_index,u.y).c;
        while(next_tile == ' ')
        {
          map[convert_to_index(u.x-vision_index,u.y)].c = 'H';
          map[convert_to_index(u.x-vision_index,u.y)].id = u.id;
          vision_index++;
          next_tile = get_element(u.x-vision_index,u.y).c;
        }
        break;
      case 'N':
        // Fill vision line to the bottom
        vision_index = 1;
        next_tile = get_element(u.x,u.y+vision_index).c;
        while(next_tile == ' ')
        {
          map[convert_to_index(u.x,u.y+vision_index)].c = 'H';
          map[convert_to_index(u.x,u.y+vision_index)].id = u.id;
          vision_index++;
          next_tile = get_element(u.x,u.y+vision_index).c;
        }
        break;
      case 'J':
        // Fill vision line to the right
        vision_index = 1;
        next_tile = get_element(u.x+vision_index,u.y).c;
        while(next_tile == ' ')
        {
          map[convert_to_index(u.x+vision_index,u.y)].c = 'H';
          map[convert_to_index(u.x+vision_index,u.y)].id = u.id;
          vision_index++;
          next_tile = get_element(u.x+vision_index,u.y).c;
        }
        break;
      case 'Y':
        // Fill vision line to the top
        vision_index = 1;
        next_tile = get_element(u.x,u.y-vision_index).c;
        while(next_tile == ' ')
        {
          map[convert_to_index(u.x,u.y-vision_index)].c = 'H';
          map[convert_to_index(u.x,u.y-vision_index)].id = u.id;
          vision_index++;
          next_tile = get_element(u.x,u.y-vision_index).c;
        }
        break;
    }
  }  

  // Move deer
  overlap = get_element(x,y);
  map[convert_to_index(x,y)].c = 'D';
  deerx = x;
  deery = y;
  deeri= convert_to_index(x,y);
  return overlap; 
}

// Save map to file
void Map::save_map(string name)
{
  ofstream mapFile;
  mapFile.open(name);
  mapFile << map;
  mapFile.close();
}



string Map::toString()
{
// FMap legend:
//  'D ' is the deer
//  'B ' is a block
//  'W*' is a wolf going in * direction ( * = {u,d,l,r} )
//  'H*' is a hunter going in * direction
//  'Ex' is the exit

// Map legend:
//  'D' is the deer
//  'B' is a block
//  '2' is a wolf going up, 'S' is a wolf going down, 'E' is a wolf going right, 'Q' is a wolf going left
//  'Y' is a hunter going up, 'N' is a hunter going down, 'J' is a hunter going right, 'G' is a hunter going left
//  'X' is the exit
  stringstream toReturn;
  toReturn = "";
  for(int i = 0; i < size(); i++)
  {
    switch(map[i].c)
    {
      case 'D':
        toReturn << "[D ]";
        break;
      case 'B':
        toReturn << "[B ]";
        break;
      case '2':
        toReturn << "[Wu]";
        break;
      case 'S':
        toReturn << "[Wd]";
        break;
      case 'Q':
        toReturn << "[Wl]";
        break;
      case 'E':
        toReturn << "[Wr]";
        break;
      case 'Y':
        toReturn << "[Hu]";
        break;
      case 'N':
        toReturn << "[Hd]";
        break;
      case 'G':
        toReturn << "[Hl]";
        break;
      case 'J':
        toReturn << "[Hr]";
        break;    
      case 'X':
        toReturn << "[Ex]";
        break; 
    }

    if ((i+1)%(line_length) == 0)
    {
       toReturn << endl;
    }
  }
  return toReturn.toString();
}





int move(bool player, char direction, Map& map);
int play(bool player, Map map);





// Makes a single move in direction on map
// returns 1 if move resulted in a win
// returns 0 if move resulted in valid non win/loss state
// returns -1 if move resulted in a loss
// returns -2 if move was invalid
int move(bool player, char direction, Map& map)
{
  int deer_pos = map.deeri;
  int x = map.deerx;
  int y = map.deery;
  int new_x = 0;
  int new_y = 0;

  new_x = x;
  new_y = y;

  Unit overlap = Unit::space;

  switch (direction)
  {
    case 'w':
      if(map.get_element(x,y-1).c == 'B')
      {
        if (player) cout << "Can't move further up; path blocked.\n";
        return -2;
      }      
      else
      {
        new_y = y - 1;
        overlap = map.update_map(new_x,new_y);
      }
      break;
    case 'a':
      if(map.get_element(x-1,y).c == 'B')
      {
        if (player) cout << "Can't move further left; path blocked.\n";
        return -2;
      }      
      else
      {
        new_x = x - 1;
        overlap = map.update_map(new_x,new_y);
      }
      break;
    case 's':
      if(map.get_element(x,y+1).c == 'B')
      {
        if (player) cout << "Can't move further down; path blocked.\n";
        return -2;
      }      
      else
      {
        new_y = y + 1;
        overlap = map.update_map(new_x,new_y);
      }
      break;
    case 'd':
      if(map.get_element(x+1,y).c == 'B')
      {
        if (player) cout << "Can't move further right; path blocked.\n";
        return -2;
      }      
      else
      {
        new_x = x + 1;
        overlap = map.update_map(new_x,new_y);
      }
      break;
    default:
      cout << "Invalid input: wasd only.\n";
  }
  if (overlap.c == 'X')
  {
    if (player) cout << "Win!\n";
	x = new_x;
	y = new_y;
    return 1;
  }
  if (overlap.c == 'Q' || overlap.c == 'E' || overlap.c == '2' || overlap.c == 'S')
  {
    if (player) cout << "Lose! To Unit #" << overlap.id << "\n";
	x = new_x;
	y = new_y;
    return -1;
  }
  // Check if the player walked through a wolf
  if (overlap.c == ' ')
  {
	  char newOccupantOfOldSpace = map.get_element(x, y).c;
	  if (newOccupantOfOldSpace == 'Q' && direction == 'd' || // Wolf walking left, deer moved right
		  newOccupantOfOldSpace == 'E' && direction == 'a' || // Wolf walking right, deer moved left
		  newOccupantOfOldSpace == '2' && direction == 's' || // Wolf walking up, deer moved down
		  newOccupantOfOldSpace == 'S' && direction == 'w')   // Wolf walking down, deer moved up 
	  {
		  // Player switched places with a wolf
		  if (player) cout << "Lose!";
		  x = new_x;
		  y = new_y;
		  return -1;
	  }
  }
  if (overlap.c == 'G' || overlap.c == 'J' || overlap.c == 'Y' || overlap.c == 'N' || overlap.c == 'H')
  {
    if (player )cout << "Lose! To Unit #" << overlap.id << "\n";
	x = new_x;
	y = new_y;
    return -1;
  }
  x = new_x;
  y = new_y;
  return 0;
}



// Play the game using terminal input/output (if player is true) or with moves if player is false
// Returns -1 on loss, 0 on stall out, number of moves taken to win on win
int play(bool player, Map map)
{

  // Copy map to test moves before making them.
  Map test(map);


  stringstream input;
  int rnd;
  int moves = 0;
  int wchance = 25;
  int achance = 25;
  int schance = 25;
  int dchance = 25;
  int wvalid = 0;
  int avalid = 0;
  int svalid = 0;
  int dvalid = 0;
  int wresult = 0;
  int aresult = 0;
  int sresult = 0;
  int dresult = 0;
  int num_valid = 0;

  if (player) map.print_map();
  int move_result = 0;
  char tmp;
  while (move_result == 0 || move_result == -2)
  {
    // Check validity of each direction of movement
    test = map;
    wresult = move(false, 'w', test);
    test = map;
    aresult = move(false, 'a', test);
    test = map;
    sresult = move(false, 's', test);
    test = map;
    dresult = move(false, 'd', test);

    // Reset random chances
    wchance = 25;
    achance = 25;
    schance = 25;
    dchance = 25;

    if(wresult < 0) // moving w will lose or is invalid
    {
      wvalid = 0;
    } 
    else // Moving is valid, but won't win
    {
      wvalid = 1;
    }

    if(aresult < 0) // moving a will lose or is invalid
    {
      avalid = 0;
    } 
    else // Moving is valid, but won't win
    {
      avalid = 1;
    }

    if(sresult < 0) // moving s will lose or is invalid
    {
      svalid = 0;
    } 
    else // Moving is valid, but won't win
    {
      svalid = 1;
    }

    if(dresult < 0) // moving d will lose or is invalid
    {
      dvalid = 0;
    } 
    else // Moving is valid, but won't win
    {
      dvalid = 1;
    }


    if(wresult == 1) // Moving w will win
    {
      wvalid = 1;
      avalid = 0;
      svalid = 0;
      dvalid = 0;
    }
    if(aresult == 1) // Moving a will win
    {
      wvalid = 0;
      avalid = 1;
      svalid = 0;
      dvalid = 0;
    }
    if(sresult == 1) // Moving s will win
    {
      wvalid = 0;
      avalid = 0;
      svalid = 1;
      dvalid = 0;
    }
    if(dresult == 1) // Moving d will win
    {
      wvalid = 0;
      avalid = 0;
      svalid = 0;
      dvalid = 1;
    }

    num_valid = wvalid + avalid + svalid + dvalid;

    // There are no valid moves
    if (num_valid == 0)
    {
      num_valid = 1;   // Force an up move. This will either stall out or lose.
    }

    // This will force chance to be 0 if the move is invalid
    // And otherwise split 100% evenly among valid moves
    wchance = wvalid * 100/num_valid;
    achance = avalid * 100/num_valid;
    schance = svalid * 100/num_valid;
    dchance = dvalid * 100/num_valid;
  
    // Get random input from valid options

    rnd = rand() % 100;
    if(rnd < wchance)
    {
      input << "w";
    }
    else if (rnd < achance + wchance)
    {
      input << "a";
    }
    else if (rnd < schance + achance + wchance)
    {
      input << "s";
    }
    else
    {
      input << "d";
    }

  
    // Get direction of movement
    if (player) cout << "Enter a wasd direction.\n";
    if (player) cin >> tmp;
    else input >> tmp;

    // Make the move
    move_result = move(player,tmp,map);

    // Update moves
    moves++;

    // Print map if player is playing
    if(player && move_result == 0) map.print_map();

    // Stall out if generated moves is empty
    if(player == false && (moves > 1000)) break;  // Ran out of moves; stall out
  }
  //if(move_result < 0) cout << "Move result " << move_result << " chances: " << wchance << " " << achance << " " << schance << " " << dchance << endl;
  if (move_result == -1) moves = -1;
  if (move_result == 0 || move_result == -2) moves = 0;
  return moves;
}



int main()
{
  int mu = 2;
  int theta = 2;
  int initial_mutations = 15;
  vector<Map> maps(mu+theta);
  Map map;
  int tries = 1;
  int num_tries = 100;
  int wins = 0;
  Map::ideal = 30;
  srand(time(NULL));

  bool map_found = false;


  // Generate mu+theta random individuals
  for(int i = 0; i < mu+theta; i++)
  {
    // Generate a random starting map
    maps[i].regenerate_map();
    // Advance to add obstacles
    for(int j = 0; j < initial_mutations; j++)
    {
      maps[i].advance_map();
    }
  }

  while(map_found == false)
  {
    // Evaluate individuals
    for(int i = 0; i < mu+theta; i++)
    {
      tries = 1;
      wins = 0;
      while(tries <= num_tries)
      {
        if (play(false,maps[i]) > 0) wins++;
        tries++;
      }
      maps[i].rating = 100*double(wins)/double(num_tries);
      cout << "Done! It was completed " << wins << "/" << num_tries << "(" << maps[i].rating << "\%)\n";
    }
    // Sort by rating
    sort(maps.begin(), maps.end());

    // Drop theta and evolve all
    for(int i = 0; i < mu+theta; i++)
    {
      cout << maps[i].rating << endl;
      // Found a suitable map
      if(abs(maps[i].rating-Map::ideal) < 5)
      {
        map = maps[i];
        map_found = true;
        cout << "Exiting loop. Map found!" << endl;
        break;
      }

      // Too hard
      if(maps[i].rating - Map::ideal < 0)
      {
        maps[i].simplify_map();
        cout << "Simplifying number " << i << " with rating of " << maps[i].rating << endl;
      }
      else  // Too easy
      {
        maps[i].advance_map();
        cout << "Advancing number " << i << " with rating of " << maps[i].rating << endl;
      }

      // Mutate mu of them over theta of them
      if(i >= mu)
      {
        maps[i] = maps[i-mu];
        maps[i].simplify_map();
        maps[i].simplify_map();
        maps[i].advance_map();
        maps[i].advance_map();
      }
    }
    cout << "Trying again" << endl;
  }

  map.save_map("map");  

  while(true)
  {
    play(true,map);
  }
}
