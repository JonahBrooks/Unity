using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;





public class MetalDeer : MonoBehaviour {


    public GameObject deer_prefab;
    public GameObject wolf_prefab;
    public GameObject tiger_prefab;
    public GameObject tree_prefab;
    public GameObject exit_prefab;
    public GameObject ground_prefab;

    public Text level_text;
    public static int difficulty = 70; // 50, 70, 90
    public static SaveData saveData;


    private GameObject[] model_map;
    private GameObject[] new_model_map;
    private Map current_map;
    private int map_num = 0; // 0-99
    private int last_move_angle = 90;
    private bool piecesMoving = false;


    // Private class for storing model information
    private class Model
    {
        public int x, y, z; // Where this model was before the update
        public int newx, newy, newz; // Where this model will be after the update

        public char c; // What type this model was
        public char newc; // What type this model will be

        public int angleFacing;
        public int newAngleFacing;
        public GameObject model;

        public static List<Model> models;

        public Model(char c, int x, int y, int z)
        {
            this.c = c;
            this.x = x;
            this.y = y;
            this.z = z;
            newc = c;
            newx = x;
            newy = y;
            newz = z;
            angleFacing = 0;
            newAngleFacing = 0;
        }

        public static Model GetModelByOldXYZ(int oldx, int oldy, int oldz)
        {
            foreach(var m in models)
            {
                if(m.x == oldx && m.y == oldy && m.z == oldz)
                {
                    return m;
                }
            }
            return null;
        }

    }

    // Private class for storing unit information
    private class Unit
    {
        public static int total_Units;
        public static Unit space = new Unit(' ',-1);
        public static Unit edge = new Unit('B',-2);
        public static Unit exit = new Unit('X', -3);
        public static Unit deer = new Unit('D', -4);
        public int x;
        public int y;
        public char c;
        public int id;

        public Unit()
        {
            c = ' ';
            id = -1;
        }

        public Unit(char type)
        {
            c = type;
            id = Unit.total_Units++;
        }

        public Unit(char type, int i)
        {
            c = type;
            id = i;
        }
    }

    // Private class for storing map information
    private class Map : IComparable
    {
        public static float ideal = 50;
        public int line_length;
        public int deeri;
        public int deerx;
        public int deery;
        public int exiti;
        public int exitx;
        public int exity;
        public float rating;
        public Unit[] map;

        public Map()
        {
            map = new Unit[16 * 9];
            line_length = 16;
            regenerate_map();
            rating = 101;
        }

        public Map(int size, int line_size)
        {
            map = new Unit[size];
            line_length = line_size;
            regenerate_map();
            rating = 101;
        }

       
        // Loads a map from a file
        public Map(string file_name)
        {
            char one, two, three, four;
            Queue<char> raw_map = new Queue<char>();
            int raw_map_size;
            Queue<Unit> new_map = new Queue<Unit>();

            TextAsset txtAsset = (TextAsset)Resources.Load(file_name, typeof(TextAsset));
            string [] lines = Regex.Split(txtAsset.text, "\n|\r|\n\r");
            
            line_length = lines[0].Length / 4;
            foreach(string line in lines)
            {
                for (int i = 0; i < line.Length; i++)
                {
                    raw_map.Enqueue(line[i]);
                }
            }

            raw_map_size = raw_map.Count;
            for (int i = 0; i < raw_map_size; i += 4)
            {
                one = raw_map.Dequeue();
                two = raw_map.Dequeue();
                three = raw_map.Dequeue();
                four = raw_map.Dequeue();

                // Validate input
                if (one == '[' && four == ']')
                {
                    if (two == ' ')
                    {

                        // Empty space
                        new_map.Enqueue(new Unit(' ',Unit.space.id));
                    }
                    else if (two == 'D')
                    {
                        // Deer
                        new_map.Enqueue(Unit.deer);
                        deeri = new_map.Count - 1; ;
                        convert_from_index(out deerx, out deery, deeri);
                    }
                    else if (two == 'E')
                    {
                        // Exit
                        new_map.Enqueue(Unit.exit);
                        exiti = new_map.Count - 1;
                        convert_from_index(out exitx, out exity, exiti);
                    }
                    else if (two == 'B')
                    {
                        new_map.Enqueue(Unit.edge);
                    }
                    else if (two == 'W')
                    {
                        // Wolf
                        if (three == 'l')
                        {
                            new_map.Enqueue(new Unit('Q'));
                        }
                        else if (three == 'r')
                        {
                            new_map.Enqueue(new Unit('E'));
                        }
                        else if (three == 'u')
                        {
                            new_map.Enqueue(new Unit('2'));
                        }
                        else if (three == 'd')
                        {
                            new_map.Enqueue(new Unit('S'));
                        }
                    }
                    else if (two == 'H')
                    {
                        // Hunter
                        if (three == 'l')
                        {
                            new_map.Enqueue(new Unit('G'));
                        }
                        else if (three == 'r')
                        {
                            new_map.Enqueue(new Unit('J'));
                        }
                        else if (three == 'u')
                        {
                            new_map.Enqueue(new Unit('Y'));
                        }
                        else if (three == 'd')
                        {
                            new_map.Enqueue(new Unit('N'));
                        }
                    }
                }
            }
            rating = 101;
            map = new Unit[new_map.Count];
            for (int i = 0; i < size(); i++)
            {
                map[i] = new_map.Dequeue();
            }

        }

        public Map(ref Map old_map)
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

        public int size()
        {
            return map.Length;
        }

        // Returns the index representing x,y
        public int convert_to_index(int x, int y)
        {
            int i = 0;
            i = x;
            i += y * line_length;
            return i;
        }

        // Returns the x,y coordinates represented by index i
        public void convert_from_index(out int x, out int y, int i)
        {
            x = i % (line_length);
            y = i / (line_length);
        }

        // Generates a new, empty map with the deer in one corner and the exit in the other.
        public void regenerate_map()
        {
            int deer_index = 0;
            int exit_index = size() - 1;

            deer_index = 0; // Random.Range(0,size());
            exit_index = size() - 1; // Random.Range(0,size());

            while (deer_index == exit_index && size() > 1)
            {
                exit_index = Random.Range(0,size());
            }

            for (int i = 0; i < size(); i++)
            {
                if (i == deer_index)
                {
                    map[i] = Unit.deer;
                    deeri = i;
                    convert_from_index(out deerx, out deery, deeri);
                }
                else if (i == exit_index)
                {
                    map[i] = Unit.exit;
                    exiti = i;
                    convert_from_index(out exitx, out exity, exiti);
                }
                else
                {
                    map[i] = Unit.space;
                }
            }
            rating = 101;
        }

        // Returns the unit at given x,y coordinates
        public Unit get_element(int x, int y)
        {
            int i = convert_to_index(x, y);
            if (i < 0 || i >= size() || x < 0 || x >= line_length)
            {
                return Unit.edge;
            }
            else
            {
                return map[i];
            }
        }


        // Returns overlap (the character that was replaced by D after updates)
        public char update_map(int x, int y)
        {
            char overlap = ' ';

            int vision_index = 0;
            int old_x = deerx;
            int old_y = deery;

            char next_tile;
            Unit u = new Unit(' ');

            Queue<Unit> q = new Queue<Unit>(); // For the first pass of movement
            Queue<Unit> q2 = new Queue<Unit>();// For the second pass of movement
            Queue<Unit> q3 = new Queue<Unit>();// For hunter vision lines

            // Clear Deer from old position
            map[convert_to_index(old_x, old_y)].c = ' ';
            map[convert_to_index(old_x, old_y)].id = Unit.space.id;
            
            // Move wolves and hunters 
            // Fill queue
            for (int i = 0; i < size(); i++)
            {
                convert_from_index(out (map[i].x), out (map[i].y), i);
                if (map[i].c == 'Q' || map[i].c == 'S' || map[i].c == 'E' || map[i].c == '2')
                {
                    q.Enqueue(map[i]);
                }
                if (map[i].c == 'G' || map[i].c == 'N' || map[i].c == 'J' || map[i].c == 'Y')
                {
                    q.Enqueue(map[i]);
                }
                if (map[i].c == 'H') // Hunter vision line
                {
                    map[i].c = ' '; // Clear hunter vision lines
                    map[i].id = Unit.space.id;
                }
            }
            // Parse queue
            while (q.Count > 0)
            {
                u = q.Dequeue();
            
                switch (u.c)
                {
                    case 'Q': // Wolf left
                        next_tile = get_element(u.x - 1, u.y).c;
                        if (next_tile != ' ' && next_tile != 'H')
                        {
                            // Turn around
                            q2.Enqueue(map[convert_to_index(u.x, u.y)]);
                        }
                        else
                        {
                            Model.GetModelByOldXYZ(u.x, 0, u.y).newx -= 1;
                            map[convert_to_index(u.x - 1, u.y)].c = 'Q';
                            map[convert_to_index(u.x - 1, u.y)].id = u.id;
                            map[convert_to_index(u.x, u.y)].c = ' ';
                            map[convert_to_index(u.x, u.y)].id = Unit.space.id;
                        }
                        break;
                    case 'S': // Wolf down
                        next_tile = get_element(u.x, u.y + 1).c;
                        if (next_tile != ' ' && next_tile != 'H')
                        {
                            // Turn around
                            q2.Enqueue(map[convert_to_index(u.x, u.y)]);
                        }
                        else
                        {
                            Model.GetModelByOldXYZ(u.x, 0, u.y).newz += 1;
                            map[convert_to_index(u.x, u.y + 1)].c = 'S';
                            map[convert_to_index(u.x, u.y + 1)].id = u.id;
                            map[convert_to_index(u.x, u.y)].c = ' ';
                            map[convert_to_index(u.x, u.y)].id = Unit.space.id;
                        }
                        break;
                    case 'E': // Wolf right
                        next_tile = get_element(u.x + 1, u.y).c;
                        if (next_tile != ' ' && next_tile != 'H')
                        {
                            // Turn around
                            q2.Enqueue(map[convert_to_index(u.x, u.y)]);
                        }
                        else
                        {
                            Model.GetModelByOldXYZ(u.x, 0, u.y).newx += 1;
                            map[convert_to_index(u.x + 1, u.y)].c = 'E';
                            map[convert_to_index(u.x + 1, u.y)].id = u.id;
                            map[convert_to_index(u.x, u.y)].c = ' ';
                            map[convert_to_index(u.x, u.y)].id = Unit.space.id;
                        }
                        break;
                    case '2': // Wolf up
                        next_tile = get_element(u.x, u.y - 1).c;
                        if (next_tile != ' ' && next_tile != 'H')
                        {
                            // Turn around
                            q2.Enqueue(map[convert_to_index(u.x, u.y)]);
                        }
                        else
                        {
                            Model.GetModelByOldXYZ(u.x, 0, u.y).newz -= 1;
                            map[convert_to_index(u.x, u.y - 1)].c = '2';
                            map[convert_to_index(u.x, u.y - 1)].id = u.id;
                            map[convert_to_index(u.x, u.y)].c = ' ';
                            map[convert_to_index(u.x, u.y)].id = Unit.space.id;
                        }
                        break;
                    case 'G': // Hunter left
                        next_tile = get_element(u.x - 1, u.y).c;
                        if (next_tile != ' ' && next_tile != 'H')
                        {
                            // Turn around
                            q2.Enqueue(map[convert_to_index(u.x, u.y)]);
                        }
                        else
                        {
                            Model.GetModelByOldXYZ(u.x, 0, u.y).newx -= 1;
                            map[convert_to_index(u.x - 1, u.y)].c = 'G';
                            map[convert_to_index(u.x - 1, u.y)].id = u.id;
                            map[convert_to_index(u.x, u.y)].c = ' ';
                            map[convert_to_index(u.x, u.y)].id = Unit.space.id;
                            // Prep to draw vision lines
                            q3.Enqueue(map[convert_to_index(u.x - 1, u.y)]);
                        }
                        break;
                    case 'N': // Hunter down
                        next_tile = get_element(u.x, u.y + 1).c;
                        if (next_tile != ' ' && next_tile != 'H')
                        {
                            // Turn around
                            q2.Enqueue(map[convert_to_index(u.x, u.y)]);
                        }
                        else
                        {
                            Model.GetModelByOldXYZ(u.x, 0, u.y).newz += 1;
                            map[convert_to_index(u.x, u.y + 1)].c = 'N';
                            map[convert_to_index(u.x, u.y + 1)].id = u.id;
                            map[convert_to_index(u.x, u.y)].c = ' ';
                            map[convert_to_index(u.x, u.y)].id = Unit.space.id;
                            // Prep to draw vision lines
                            q3.Enqueue(map[convert_to_index(u.x, u.y + 1)]);
                        }
                        break;
                    case 'J': // Hunter right
                        next_tile = get_element(u.x + 1, u.y).c;
                        if (next_tile != ' ' && next_tile != 'H')
                        {
                            // Turn around
                            q2.Enqueue(map[convert_to_index(u.x, u.y)]);
                        }
                        else
                        {
                            Model.GetModelByOldXYZ(u.x, 0, u.y).newx += 1;
                            map[convert_to_index(u.x + 1, u.y)].c = 'J';
                            map[convert_to_index(u.x + 1, u.y)].id = u.id;
                            map[convert_to_index(u.x, u.y)].c = ' ';
                            map[convert_to_index(u.x, u.y)].id = Unit.space.id;
                            // Prep to draw vision lines
                            q3.Enqueue(map[convert_to_index(u.x + 1, u.y)]);
                        }
                        break;
                    case 'Y': // Hunter up
                        next_tile = get_element(u.x, u.y - 1).c;
                        if (next_tile != ' ' && next_tile != 'H')
                        {
                            // Turn around
                            q2.Enqueue(map[convert_to_index(u.x, u.y)]);
                        }
                        else
                        {
                            Model.GetModelByOldXYZ(u.x, 0, u.y).newz -= 1;
                            map[convert_to_index(u.x, u.y - 1)].c = 'Y';
                            map[convert_to_index(u.x, u.y - 1)].id = u.id;
                            map[convert_to_index(u.x, u.y)].c = ' ';
                            map[convert_to_index(u.x, u.y)].id = Unit.space.id;
                            // Prep to draw vision lines
                            q3.Enqueue(map[convert_to_index(u.x, u.y - 1)]);
                        }
                        break;
                }
            }
            // Parse queue
            while (q2.Count > 0)
            {
                u = q2.Dequeue();
            
                switch (u.c)
                {
                    case 'Q':
                        next_tile = get_element(u.x - 1, u.y).c;
                        if (next_tile != ' ' && next_tile != 'H')
                        {
                            // Turn around
                            Model.GetModelByOldXYZ(u.x, 0, u.y).newc = 'E';
                            map[convert_to_index(u.x, u.y)].c = 'E';
                        }
                        else
                        {
                            Model.GetModelByOldXYZ(u.x, 0, u.y).newx -= 1;
                            map[convert_to_index(u.x - 1, u.y)].c = 'Q';
                            map[convert_to_index(u.x - 1, u.y)].id = u.id;
                            map[convert_to_index(u.x, u.y)].c = ' ';
                            map[convert_to_index(u.x, u.y)].id = Unit.space.id;
                        }
                        break;
                    case 'S':
                        next_tile = get_element(u.x, u.y + 1).c;
                        if (next_tile != ' ' && next_tile != 'H')
                        {
                            // Turn around
                            Model.GetModelByOldXYZ(u.x, 0, u.y).newc = '2';
                            map[convert_to_index(u.x, u.y)].c = '2';
                        }
                        else
                        {
                            Model.GetModelByOldXYZ(u.x, 0, u.y).newz += 1;
                            map[convert_to_index(u.x, u.y + 1)].c = 'S';
                            map[convert_to_index(u.x, u.y + 1)].id = u.id;
                            map[convert_to_index(u.x, u.y)].c = ' ';
                            map[convert_to_index(u.x, u.y)].id = Unit.space.id;
                        }
                        break;
                    case 'E':
                        next_tile = get_element(u.x + 1, u.y).c;
                        if (next_tile != ' ' && next_tile != 'H')
                        {
                            // Turn around
                            Model.GetModelByOldXYZ(u.x, 0, u.y).newc = 'Q';
                            map[convert_to_index(u.x, u.y)].c = 'Q';
                        }
                        else
                        {
                            Model.GetModelByOldXYZ(u.x, 0, u.y).newx += 1;
                            map[convert_to_index(u.x + 1, u.y)].c = 'E';
                            map[convert_to_index(u.x + 1, u.y)].id = u.id;
                            map[convert_to_index(u.x, u.y)].c = ' ';
                            map[convert_to_index(u.x, u.y)].id = Unit.space.id;
                        }
                        break;
                    case '2':
                        next_tile = get_element(u.x, u.y - 1).c;
                        if (next_tile != ' ' && next_tile != 'H')
                        {
                            // Turn around
                            Model.GetModelByOldXYZ(u.x, 0, u.y).newc = 'S';
                            map[convert_to_index(u.x, u.y)].c = 'S';
                        }
                        else
                        {
                            Model.GetModelByOldXYZ(u.x, 0, u.y).newz -= 1;
                            map[convert_to_index(u.x, u.y - 1)].c = '2';
                            map[convert_to_index(u.x, u.y - 1)].id = u.id;
                            map[convert_to_index(u.x, u.y)].c = ' ';
                            map[convert_to_index(u.x, u.y)].id = Unit.space.id;
                        }
                        break;
                    case 'G':
                        next_tile = get_element(u.x - 1, u.y).c;
                        if (next_tile != ' ' && next_tile != 'H')
                        {
                            // Turn around
                            Model.GetModelByOldXYZ(u.x, 0, u.y).newc = 'J';
                            map[convert_to_index(u.x, u.y)].c = 'J';
                            // Prep to draw vision lines
                            q3.Enqueue(map[convert_to_index(u.x, u.y)]);
                        }
                        else
                        {
                            Model.GetModelByOldXYZ(u.x, 0, u.y).newx -= 1;
                            map[convert_to_index(u.x - 1, u.y)].c = 'G';
                            map[convert_to_index(u.x - 1, u.y)].id = u.id;
                            map[convert_to_index(u.x, u.y)].c = ' ';
                            map[convert_to_index(u.x, u.y)].id = Unit.space.id;
                            // Prep to draw vision lines
                            q3.Enqueue(map[convert_to_index(u.x - 1, u.y)]);
                        }
                        break;
                    case 'N':
                        next_tile = get_element(u.x, u.y + 1).c;
                        if (next_tile != ' ' && next_tile != 'H')
                        {
                            // Turn around
                            Model.GetModelByOldXYZ(u.x, 0, u.y).newc = 'Y';
                            map[convert_to_index(u.x, u.y)].c = 'Y';
                            // Prep to draw vision lines
                            q3.Enqueue(map[convert_to_index(u.x, u.y)]);
                        }
                        else
                        {
                            Model.GetModelByOldXYZ(u.x, 0, u.y).newz += 1;
                            map[convert_to_index(u.x, u.y + 1)].c = 'N';
                            map[convert_to_index(u.x, u.y + 1)].id = u.id;
                            map[convert_to_index(u.x, u.y)].c = ' ';
                            map[convert_to_index(u.x, u.y)].id = Unit.space.id;
                            // Prep to draw vision lines
                            q3.Enqueue(map[convert_to_index(u.x, u.y + 1)]);
                        }
                        break;
                    case 'J':
                        next_tile = get_element(u.x + 1, u.y).c;
                        if (next_tile != ' ' && next_tile != 'H')
                        {
                            // Turn around
                            Model.GetModelByOldXYZ(u.x, 0, u.y).newc = 'G';
                            map[convert_to_index(u.x, u.y)].c = 'G';
                            // Prep to draw vision lines
                            q3.Enqueue(map[convert_to_index(u.x, u.y)]);
                        }
                        else
                        {
                            Model.GetModelByOldXYZ(u.x, 0, u.y).newx += 1;
                            map[convert_to_index(u.x + 1, u.y)].c = 'J';
                            map[convert_to_index(u.x + 1, u.y)].id = u.id;
                            map[convert_to_index(u.x, u.y)].c = ' ';
                            map[convert_to_index(u.x, u.y)].id = Unit.space.id;
                            // Prep to draw vision lines
                            q3.Enqueue(map[convert_to_index(u.x + 1, u.y)]);
                        }
                        break;
                    case 'Y':
                        next_tile = get_element(u.x, u.y - 1).c;
                        if (next_tile != ' ' && next_tile != 'H')
                        {
                            // Turn around
                            Model.GetModelByOldXYZ(u.x, 0, u.y).newc = 'N';
                            map[convert_to_index(u.x, u.y)].c = 'N';
                            // Prep to draw vision lines
                            q3.Enqueue(map[convert_to_index(u.x, u.y)]);
                        }
                        else
                        {
                            Model.GetModelByOldXYZ(u.x, 0, u.y).newz -= 1;
                            map[convert_to_index(u.x, u.y - 1)].c = 'Y';
                            map[convert_to_index(u.x, u.y - 1)].id = u.id;
                            map[convert_to_index(u.x, u.y)].c = ' ';
                            map[convert_to_index(u.x, u.y)].id = Unit.space.id;
                            // Prep to draw vision lines
                            q3.Enqueue(map[convert_to_index(u.x, u.y - 1)]);
                        }
                        break;
                }
            }
            // Parse queue
            while (q3.Count > 0)
            {
                u = q3.Dequeue();
            
                switch (u.c)
                {
                    case 'G':
                        // Fill vision line to the left
                        vision_index = 1;
                        next_tile = get_element(u.x - vision_index, u.y).c;
                        while (next_tile == ' ' || next_tile == 'H')
                        {
                            map[convert_to_index(u.x - vision_index, u.y)].c = 'H';
                            map[convert_to_index(u.x - vision_index, u.y)].id = u.id;
                            vision_index++;
                            next_tile = get_element(u.x - vision_index, u.y).c;
                        }
                        break;
                    case 'N':
                        // Fill vision line to the bottom
                        vision_index = 1;
                        next_tile = get_element(u.x, u.y + vision_index).c;
                        while (next_tile == ' ' || next_tile == 'H')
                        {
                            map[convert_to_index(u.x, u.y + vision_index)].c = 'H';
                            map[convert_to_index(u.x, u.y + vision_index)].id = u.id;
                            vision_index++;
                            next_tile = get_element(u.x, u.y + vision_index).c;
                        }
                        break;
                    case 'J':
                        // Fill vision line to the right
                        vision_index = 1;
                        next_tile = get_element(u.x + vision_index, u.y).c;
                        while (next_tile == ' ' || next_tile == 'H')
                        {
                            map[convert_to_index(u.x + vision_index, u.y)].c = 'H';
                            map[convert_to_index(u.x + vision_index, u.y)].id = u.id;
                            vision_index++;
                            next_tile = get_element(u.x + vision_index, u.y).c;
                        }
                        break;
                    case 'Y':
                        // Fill vision line to the top
                        vision_index = 1;
                        next_tile = get_element(u.x, u.y - vision_index).c;
                        while (next_tile == ' ' || next_tile == 'H')
                        {
                            map[convert_to_index(u.x, u.y - vision_index)].c = 'H';
                            map[convert_to_index(u.x, u.y - vision_index)].id = u.id;
                            vision_index++;
                            next_tile = get_element(u.x, u.y - vision_index).c;
                        }
                        break;
                }
            }

            // Move deer
            overlap = get_element(x, y).c;

            // Prepare to move deer model
            Model.GetModelByOldXYZ(old_x, 0, old_y).newx = x;
            Model.GetModelByOldXYZ(old_x, 0, old_y).newz = y;

            map[convert_to_index(x, y)].c = 'D';
            map[convert_to_index(x, y)].id = Unit.deer.id;
            deerx = x;
            deery = y;
            deeri = convert_to_index(x, y);
            return overlap;
        }

        public static bool operator <(Map lhs, Map rhs)
        {
            return Mathf.Abs(lhs.rating - Map.ideal) < Mathf.Abs(rhs.rating - Map.ideal);
        }
        public static bool operator >(Map lhs, Map rhs)
        {
            return Mathf.Abs(lhs.rating - Map.ideal) > Mathf.Abs(rhs.rating - Map.ideal);
        }

        public int CompareTo(object arg)
        {
            Map that = (Map)arg;
            if (this < that)
            {
                return -1;
            }
            else if ( this > that)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

    }


    // Makes a single move in direction on map
    // returns 1 if move resulted in a win
    // returns 0 if move resulted in valid non win/loss state
    // returns -1 if move resulted in a loss
    // returns -2 if move was invalid
    int move(bool player, char direction, ref Map map)
    {
        //TODO: Display the errors somewhere other than Debug.Log

        int x = map.deerx;
        int y = map.deery;
        int new_x = 0;
        int new_y = 0;

        new_x = x;
        new_y = y;

        char overlap = ' ';

        switch (direction)
        {
            case 'w':
                if (map.get_element(x, y - 1).c == 'B')
                {
                    if (player) Debug.Log("Can't move further up; path blocked.");
                    return -2;
                }
                else
                {
                    new_y = y - 1;
                    overlap = map.update_map(new_x, new_y);
                }
                break;
            case 'a':
                if (map.get_element(x - 1, y).c == 'B')
                {
                    if (player) Debug.Log("Can't move further left; path blocked.");
                    return -2;
                }
                else
                {
                    new_x = x - 1;
                    overlap = map.update_map(new_x, new_y);
                }
                break;
            case 's':
                if (map.get_element(x, y + 1).c == 'B')
                {
                    if (player) Debug.Log("Can't move further down; path blocked.");
                    return -2;
                }
                else
                {
                    new_y = y + 1;
                    overlap = map.update_map(new_x, new_y);
                }
                break;
            case 'd':
                if (map.get_element(x + 1, y).c == 'B')
                {
                    if (player) Debug.Log("Can't move further right; path blocked.");
                    return -2;
                }
                else
                {
                    new_x = x + 1;
                    overlap = map.update_map(new_x, new_y);
                }
                break;
        }

        x = new_x;
        y = new_y;
        if (overlap == 'X')
        {
            if (player) Debug.Log("Win!");
            return 1;
        }
        if (overlap == 'Q' || overlap == 'E' || overlap == '2' || overlap == 'S')
        {
            if (player) Debug.Log("Lose!");
            return -1;
        }
        if (overlap == 'G' || overlap == 'J' || overlap == 'Y' || overlap == 'N' || overlap == 'H')
        {
            if (player) Debug.Log("Lose!");
            return -1;
        }
        return 0;


    }


    // Used for debugging; shows the raw text map on the screen
    void text_map(Map map)
    {
        level_text.text = "";
        for (int i = 0; i < map.size(); i++)
        {
            level_text.text += map.map[i].c;
            if ((i + 1) % map.line_length == 0)
            {
                level_text.text += "\n";
            }
        }
    }
    


    // Use this for initialization
    void Start () {
        string map_string;

        saveData = SaveData.Load();

        // If there was no save data
        if(saveData == null)
        {
            // Create a new empty save file
            saveData = new SaveData();
            SaveData.Save(saveData);
        }


        if(difficulty == 90) // Easy
        {
            map_num = saveData.EasyLevel;
        }
        else if (difficulty == 70) // Normal
        {
            map_num = saveData.NormalLevel;
        }
        else // Hard
        {
            map_num = saveData.HardLevel;
        }

        level_text.text = "Map " + (map_num + 1) + "/100";

        map_string = "Maps/polar_maps/" + difficulty + "/map" + difficulty + "_" + map_num;
        
        // Fetch new map
        current_map = new Map(map_string);
        // Position deer and exit correctly for new map
        current_map.deeri = 0;
        current_map.deerx = 0;
        current_map.deery = 0;
        current_map.map[0].c = 'D';
        current_map.map[0].id = Unit.deer.id;
        current_map.exiti = current_map.size() - 1;
        current_map.convert_from_index(out current_map.exitx, out current_map.exity, current_map.exiti);
        current_map.map[current_map.exiti].c = 'X';
        current_map.map[current_map.exiti].id = Unit.exit.id;

        model_map = new GameObject[current_map.size()];
        Model.models = new List<Model>();

        // Build ground grid
        for(int i = 0; i < 16; i++)
        {
            for(int j = 0; j < 9; j++)
            {
                Instantiate(ground_prefab, new Vector3(i, -0.5f, j), Quaternion.identity);
            }
        }

        ResetAllModels();
    }
	
    IEnumerator MoveAllPieces()
    {
        piecesMoving = true;

        float stepsToComplete = 10f;

        float step = 1f / stepsToComplete; ;

        // Rotate deer instantly
        foreach (var m in Model.models)
        {
            if (m.c == 'D')
            {
                m.model.transform.eulerAngles = new Vector3(0, last_move_angle, 0);
            }
        }

        for (float f = 0; f <= 1; f+= step)
        {
            foreach(var m in Model.models)
            {
                
                // Rotate if newc != c
                if(m.newc != m.c)
                {
                    switch (m.newc)
                    {
                        case '2':
                            m.newAngleFacing = 180;
                            break;
                        case 'Q':
                            m.newAngleFacing = -90;
                            break;
                        case 'E':
                            m.newAngleFacing = 90;
                            break;
                        case 'S':
                            m.newAngleFacing = 0;
                            break;
                        case 'Y':
                            m.newAngleFacing = 180;
                            break;
                        case 'G':
                            m.newAngleFacing = -90;
                            break;
                        case 'N':
                            m.newAngleFacing = 0;
                            break;
                        case 'J':
                            m.newAngleFacing = 90;
                            break;
                    }
                }
                // Rotate everything but the deer
                if (m.newc != 'D')
                {
                    m.model.transform.rotation = Quaternion.Lerp(Quaternion.Euler(0, m.angleFacing, 0), Quaternion.Euler(0, m.newAngleFacing,0), f);
                }
                m.model.transform.position = Vector3.MoveTowards(m.model.transform.position, new Vector3(m.newx, m.newy, m.newz), step);
                m.x = m.newx;
                m.y = m.newy;
                m.z = m.newz;
                m.c = m.newc;
            }
            
            yield return new WaitForSeconds(.005f);
        }

        // Update the angleFacing, now that they are facing the right angles
        foreach(var m in Model.models)
        {
            m.angleFacing = m.newAngleFacing;
        }
        piecesMoving = false;
    }

    // Resets all the models based on their map positions
    void ResetAllModels()
    {
        foreach(var m in Model.models)
        {
            Destroy(m.model);
        }
        Model.models = new List<Model>();

        int x, y, z;
        y = 0;

        for (int i = 0; i < current_map.size(); i++)
        {
            current_map.convert_from_index(out x, out z, i);

            switch (current_map.get_element(x, z).c)
            {
                case ' ':
                    model_map[i] = null;
                    break;
                case 'D':
                    model_map[i] = Instantiate(deer_prefab, new Vector3(x, y, z), Quaternion.Euler(0, 90, 0));
                    break;
                case '2':
                    model_map[i] = Instantiate(wolf_prefab, new Vector3(x, y, z), Quaternion.Euler(0, 180, 0));
                    break;
                case 'Q':
                    model_map[i] = Instantiate(wolf_prefab, new Vector3(x, y, z), Quaternion.Euler(0, -90, 0));
                    break;
                case 'E':
                    model_map[i] = Instantiate(wolf_prefab, new Vector3(x, y, z), Quaternion.Euler(0, 90, 0));
                    break;
                case 'S':
                    model_map[i] = Instantiate(wolf_prefab, new Vector3(x, y, z), Quaternion.Euler(0, 0, 0));
                    break;
                case 'Y':
                    model_map[i] = Instantiate(tiger_prefab, new Vector3(x, y, z), Quaternion.Euler(0, 180, 0));
                    break;
                case 'G':
                    model_map[i] = Instantiate(tiger_prefab, new Vector3(x, y, z), Quaternion.Euler(0, -90, 0));
                    break;
                case 'N':
                    model_map[i] = Instantiate(tiger_prefab, new Vector3(x, y, z), Quaternion.Euler(0, 0, 0));
                    break;
                case 'J':
                    model_map[i] = Instantiate(tiger_prefab, new Vector3(x, y, z), Quaternion.Euler(0, 90, 0));
                    break;
                case 'B':
                    model_map[i] = Instantiate(tree_prefab, new Vector3(x, y, z), Quaternion.identity);
                    break;
                case 'X':
                    model_map[i] = Instantiate(exit_prefab, new Vector3(x, y, z), Quaternion.identity);
                    break;
            }
            // Fill the models list with all pieces besides bushes, spaces, and the exit
            if (current_map.get_element(x, z).c != ' ' && current_map.get_element(x, z).c != 'B' && current_map.get_element(x, z).c != 'X')
            {
                Model.models.Add(new Model(current_map.get_element(x, z).c, x, y, z));
                Model.models[Model.models.Count - 1].model = model_map[i];
                switch (current_map.get_element(x, z).c)
                {
                    case 'D':
                        Model.models[Model.models.Count - 1].angleFacing = 90;
                        Model.models[Model.models.Count - 1].newAngleFacing = 90;
                        break;
                    case '2':
                        Model.models[Model.models.Count - 1].angleFacing = 180;
                        Model.models[Model.models.Count - 1].newAngleFacing = 180;
                        break;
                    case 'Q':
                        Model.models[Model.models.Count - 1].angleFacing = -90;
                        Model.models[Model.models.Count - 1].newAngleFacing = -90;
                        break;
                    case 'E':
                        Model.models[Model.models.Count - 1].angleFacing = 90;
                        Model.models[Model.models.Count - 1].newAngleFacing = 90;
                        break;
                    case 'S':
                        Model.models[Model.models.Count - 1].angleFacing = 0;
                        Model.models[Model.models.Count - 1].newAngleFacing = 0;
                        break;
                    case 'Y':
                        Model.models[Model.models.Count - 1].angleFacing = 180;
                        Model.models[Model.models.Count - 1].newAngleFacing = 180;
                        break;
                    case 'G':
                        Model.models[Model.models.Count - 1].angleFacing = -90;
                        Model.models[Model.models.Count - 1].newAngleFacing = -90;
                        break;
                    case 'N':
                        Model.models[Model.models.Count - 1].angleFacing = 0;
                        Model.models[Model.models.Count - 1].newAngleFacing = 0;
                        break;
                    case 'J':
                        Model.models[Model.models.Count - 1].angleFacing = 90;
                        Model.models[Model.models.Count - 1].newAngleFacing = 90;
                        break;
                }
            }
        }
    }

	// Update is called once per frame
	void Update () {

        int move_result = 0;
        bool playerMoved = false;

        if(piecesMoving)
        {
            return; // Don't allow anything to happen while the pieces are moving
        }

        string map_string;

        // These are jumbled to account for the direction of the camera
        if (Input.GetKeyDown("down"))
        {
            move_result = move(false, 'w', ref current_map);
            last_move_angle = 180;
            playerMoved = true;
        }
        if (Input.GetKeyDown("left"))
        {
            move_result = move(false, 'a', ref current_map);
            last_move_angle = -90;
            playerMoved = true;
        }
        if (Input.GetKeyDown("up"))
        {
            move_result = move(false, 's', ref current_map);
            last_move_angle = 0;
            playerMoved = true;
        }
        if (Input.GetKeyDown("right"))
        {
            move_result = move(false, 'd', ref current_map);
            last_move_angle = 90;
            playerMoved = true;
        }
        
        // The player won
        if(move_result == 1)
        {
            if (difficulty == 90) // Easy
            {
                // Advance to the next level
                map_num = ++saveData.EasyLevel;
                if(saveData.EasyLevel >= 100)
                {   // Don't overflow the level count
                    saveData.EasyLevel = 0;
                }
            }
            else if (difficulty == 70) // Normal
            {
                map_num = ++saveData.NormalLevel;
                if (saveData.NormalLevel >= 100)
                {
                    saveData.NormalLevel = 0;
                }
            }
            else // Hard
            {
                map_num = ++saveData.HardLevel;
                if (saveData.HardLevel >= 100)
                {
                    saveData.HardLevel = 0;
                }
            }

            SaveData.Save(saveData); // Update the save file

            level_text.text = "Map " + (map_num + 1) + "/100";

            map_string = "Maps/polar_maps/" + difficulty + "/map" + difficulty + "_" + map_num;
            // Position deer and exit correctly for new map  
            current_map.deeri = 0;
            current_map.deerx = 0;
            current_map.deery = 0;
            current_map.map[0].c = 'D';
            current_map.map[0].id = Unit.deer.id;
            current_map.exiti = current_map.size() - 1;
            current_map.convert_from_index(out current_map.exitx, out current_map.exity, current_map.exiti);
            current_map.map[current_map.exiti].c = 'X';
            current_map.map[current_map.exiti].id = Unit.exit.id;
            // Fetch new map
            current_map = new Map(map_string);
            ResetAllModels();
        }
        // The player lost
        if(move_result == -1)
        {
            SceneManager.LoadScene("GameOver");
        }

        //text_map(current_map); // For debugging

        if(playerMoved)
        {
            
            StartCoroutine("MoveAllPieces");
        }


    }
}
