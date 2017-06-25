using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;





public class MetalDeer : MonoBehaviour {


    public GameObject deer_prefab;
    public GameObject wolf_prefab;
    public GameObject tiger_prefab;
    public GameObject tree_prefab;

    private GameObject[] model_map;
    private GameObject[] new_model_map;
    private Map map;

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

    // Private class for staring map information
    private class Map : IComparable
    {
        public static float ideal = 30;
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

        public Map(string file_name)
        {
            char one, two, three, four;
            Queue<char> raw_map = new Queue<char>();
            int raw_map_size;
            Queue<Unit> new_map = new Queue<Unit>();

            string[] lines = System.IO.File.ReadAllLines(file_name);
            
            foreach(string line in lines)
            {
                line_length = line.Length / 4;
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
                        new_map.Enqueue(Unit.space);
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

        public int convert_to_index(int x, int y)
        {
            int i = 0;
            i = x;
            i += y * line_length;
            return i;
        }

        public void convert_from_index(out int x, out int y, int i)
        {
            x = i % (line_length);
            y = i / (line_length);
        }

        public void regenerate_map()
        {
            int deer_index = 0;
            int exit_index = size() - 1;

            deer_index = Random.Range(0,size());
            exit_index = Random.Range(0,size());

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

        // Increase the complexity of the map by adding one element
        public void advance_map()
        {
            int type = Random.Range(0,3); // Pick Block, Wolf, or Hunter
            int direction = Random.Range(0,4); // Pick a direction for the wolf or hunter to face
            int index = Random.Range(0,size());

            // Don't overwrite deer or exit, okay to overwrite anything else
            while ((map[index].c == 'D' || map[index].c == 'X') && size() > 2)
            {
                index = Random.Range(0,size());
            }

            if (type == 0)
            {
                map[index].c = 'B';
            }
            else if (type == 1)
            {
                switch (direction)
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
                switch (direction)
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

        //Decrease the complexit of the map by removing one element
        public void simplify_map()
        {
            List<int> indexes = new List<int>();

            // Gather indexes of objects
            for (int i = 0; i < size(); i++)
            {
                if (!(map[i].c == ' ' || map[i].c == 'D' || map[i].c == 'X'))
                {
                    indexes.Add(i);
                }
            }

            if (indexes.Count > 0)
            {
                // Select one index at random and set its map position to space
                map[indexes[Random.Range(0,indexes.Count)]] = Unit.space;
            }


        }

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

        public void print_map()
        {
            Debug.Log(size());
            // TODO: Get rid of this method
            foreach(Unit unit in map)
            {
                Debug.Log(unit.c);
            }
        }

        // Returns overlap (the character that was replaced by D after updates)
        public Unit update_map(int x, int y)
        {
            Unit overlap = Unit.space; ;

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
                    case 'Q':
                        next_tile = get_element(u.x - 1, u.y).c;
                        if (next_tile != ' ' && next_tile != 'H')
                        {
                            // Turn around
                            q2.Enqueue(map[convert_to_index(u.x, u.y)]);
                        }
                        else
                        {
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
                            q2.Enqueue(map[convert_to_index(u.x, u.y)]);
                        }
                        else
                        {
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
                            q2.Enqueue(map[convert_to_index(u.x, u.y)]);
                        }
                        else
                        {
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
                            q2.Enqueue(map[convert_to_index(u.x, u.y)]);
                        }
                        else
                        {
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
                            q2.Enqueue(map[convert_to_index(u.x, u.y)]);
                        }
                        else
                        {
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
                            q2.Enqueue(map[convert_to_index(u.x, u.y)]);
                        }
                        else
                        {
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
                            q2.Enqueue(map[convert_to_index(u.x, u.y)]);
                        }
                        else
                        {
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
                            q2.Enqueue(map[convert_to_index(u.x, u.y)]);
                        }
                        else
                        {
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
                            map[convert_to_index(u.x, u.y)].c = 'E';
                        }
                        else
                        {
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
                            map[convert_to_index(u.x, u.y)].c = '2';
                        }
                        else
                        {
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
                            map[convert_to_index(u.x, u.y)].c = 'Q';
                        }
                        else
                        {
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
                            map[convert_to_index(u.x, u.y)].c = 'S';
                        }
                        else
                        {
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
                            map[convert_to_index(u.x, u.y)].c = 'J';
                            // Prep to draw vision lines
                            q3.Enqueue(map[convert_to_index(u.x, u.y)]);
                        }
                        else
                        {
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
                            map[convert_to_index(u.x, u.y)].c = 'Y';
                            // Prep to draw vision lines
                            q3.Enqueue(map[convert_to_index(u.x, u.y)]);
                        }
                        else
                        {
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
                            map[convert_to_index(u.x, u.y)].c = 'G';
                            // Prep to draw vision lines
                            q3.Enqueue(map[convert_to_index(u.x, u.y)]);
                        }
                        else
                        {
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
                            map[convert_to_index(u.x, u.y)].c = 'N';
                            // Prep to draw vision lines
                            q3.Enqueue(map[convert_to_index(u.x, u.y)]);
                        }
                        else
                        {
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
                        while (next_tile == ' ')
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
                        while (next_tile == ' ')
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
                        while (next_tile == ' ')
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
                        while (next_tile == ' ')
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
            overlap = get_element(x, y);
            map[convert_to_index(x, y)].c = 'D';
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
        int deer_pos = map.deeri;
        int x = map.deerx;
        int y = map.deery;
        int new_x = 0;
        int new_y = 0;

        new_x = x;
        new_y = y;

        Unit overlap = Unit.space;

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
        if (overlap.c == 'X')
        {
            if (player) Debug.Log("Win!");
            return 1;
        }
        if (overlap.c == 'Q' || overlap.c == 'E' || overlap.c == '2' || overlap.c == 'S')
        {
            if (player) Debug.Log("Lose! To Unit #" + overlap.id);
            return -1;
        }
        if (overlap.c == 'G' || overlap.c == 'J' || overlap.c == 'Y' || overlap.c == 'N' || overlap.c == 'H')
        {
            if (player) Debug.Log("Lose! To Unit #" + overlap.id);
            return -1;
        }
        return 0;


    }

    // TODO: Delete this entire method
    // Play the game using semi-random moves
    // Returns -1 on loss, 0 on stall out, number of moves taken to win on win
    int play(Map map)
    {

        // Copy map to test moves before making them.
        Map test = new Map(ref map);


        Queue<char> input = new Queue<char>();
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

        int move_result = 0;
        char tmp;
        while (move_result == 0 || move_result == -2)
        {
            // Check validity of each direction of movement
            test = map;
            wresult = move(false, 'w', ref test);
            test = map;
            aresult = move(false, 'a', ref test);
            test = map;
            sresult = move(false, 's', ref test);
            test = map;
            dresult = move(false, 'd', ref test);

            // Reset random chances
            wchance = 25;
            achance = 25;
            schance = 25;
            dchance = 25;

            if (wresult < 0) // moving w will lose or is invalid
            {
                wvalid = 0;
            }
            else // Moving is valid, but won't win
            {
                wvalid = 1;
            }

            if (aresult < 0) // moving a will lose or is invalid
            {
                avalid = 0;
            }
            else // Moving is valid, but won't win
            {
                avalid = 1;
            }

            if (sresult < 0) // moving s will lose or is invalid
            {
                svalid = 0;
            }
            else // Moving is valid, but won't win
            {
                svalid = 1;
            }

            if (dresult < 0) // moving d will lose or is invalid
            {
                dvalid = 0;
            }
            else // Moving is valid, but won't win
            {
                dvalid = 1;
            }


            if (wresult == 1) // Moving w will win
            {
                wvalid = 1;
                avalid = 0;
                svalid = 0;
                dvalid = 0;
            }
            if (aresult == 1) // Moving a will win
            {
                wvalid = 0;
                avalid = 1;
                svalid = 0;
                dvalid = 0;
            }
            if (sresult == 1) // Moving s will win
            {
                wvalid = 0;
                avalid = 0;
                svalid = 1;
                dvalid = 0;
            }
            if (dresult == 1) // Moving d will win
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
            wchance = wvalid * 100 / num_valid;
            achance = avalid * 100 / num_valid;
            schance = svalid * 100 / num_valid;
            dchance = dvalid * 100 / num_valid;

            // Get random input from valid options

            rnd = Random.Range(0, 100);
            if (rnd < wchance)
            {
                input.Enqueue('w');
            }
            else if (rnd < achance + wchance)
            {
                input.Enqueue('a');
            }
            else if (rnd < schance + achance + wchance)
            {
                input.Enqueue('s');
            }
            else
            {
                input.Enqueue('d');
            }


            // Get direction of movement
            tmp = input.Dequeue();

            // Make the move
            move_result = move(false, tmp, ref map);

            // Update moves
            moves++;

            // Stall out if generated moves is empty
            if (moves > 1000) break;  // Ran out of moves; stall out
        }
        if (move_result == -1) moves = -1;
        if (move_result == 0 || move_result == -2) moves = 0;
        return moves;
    }

    

    // Use this for initialization
    void Start () {
        map = new Map("Assets/Maps/polar_maps/70/map70_50");
        map.print_map();
        model_map = new GameObject[map.size()];
        int x, y, z;
        y = 0;

        for(int i = 0; i < map.size(); i++)
        {
            map.convert_from_index(out x, out z, i);
            switch(map.get_element(x,z).c)
            {
                case ' ':
                    model_map[i] = null;
                    break;
                case 'D':
                    model_map[i] = Instantiate(deer_prefab, new Vector3(x, y, z), Quaternion.Euler(0, 90, 0));
                    break;
                case '2':
                    model_map[i] = Instantiate(wolf_prefab, new Vector3(x, y, z), Quaternion.Euler(0, 0, 0));
                    break;
                case 'Q':
                    model_map[i] = Instantiate(wolf_prefab, new Vector3(x, y, z), Quaternion.Euler(0, -90, 0));
                    break;
                case 'E':
                    model_map[i] = Instantiate(wolf_prefab, new Vector3(x, y, z), Quaternion.Euler(0, 180, 0));
                    break;
                case 'S':
                    model_map[i] = Instantiate(wolf_prefab, new Vector3(x, y, z), Quaternion.Euler(0, 90, 0));
                    break;
                case 'Y':
                    model_map[i] = Instantiate(tiger_prefab, new Vector3(x, y, z), Quaternion.Euler(0, 0, 0));
                    break;
                case 'G':
                    model_map[i] = Instantiate(tiger_prefab, new Vector3(x, y, z), Quaternion.Euler(0, -90, 0));
                    break;
                case 'N':
                    model_map[i] = Instantiate(tiger_prefab, new Vector3(x, y, z), Quaternion.Euler(0, 180, 0));
                    break;
                case 'J':
                    model_map[i] = Instantiate(tiger_prefab, new Vector3(x, y, z), Quaternion.Euler(0, 90, 0));
                    break;
                case 'B':
                    model_map[i] = Instantiate(tree_prefab, new Vector3(x, y, z), Quaternion.identity);
                    break;
            }
        }
    }
	
	// Update is called once per frame
	void Update () {

        int x, y, z;
        y = 0;

        // These are jumbled to account for the direction of the camera
        if(Input.GetKeyDown("down"))
        {
            move(true, 'w', ref map);
            map.print_map();
        }
        if (Input.GetKeyDown("left"))
        {
            move(true, 'a', ref map);
            map.print_map();
        }
        if (Input.GetKeyDown("up"))
        {
            move(true, 's', ref map);
            map.print_map();
        }
        if (Input.GetKeyDown("right"))
        {
            move(true, 'd', ref map);
            map.print_map();
        }

        

        for (int i = 0; i < map.size(); i++)
        {
            map.convert_from_index(out x, out z, i);
            Destroy(model_map[i]);
            switch (map.get_element(x, z).c)
            {
                case ' ':
                    model_map[i] = null;
                    break;
                case 'D':
                    model_map[i] = Instantiate(deer_prefab, new Vector3(x, y, z), Quaternion.Euler(0, 90, 0));
                    break;
                case '2':
                    model_map[i] = Instantiate(wolf_prefab, new Vector3(x, y, z), Quaternion.Euler(0, 0, 0));
                    break;
                case 'Q':
                    model_map[i] = Instantiate(wolf_prefab, new Vector3(x, y, z), Quaternion.Euler(0, -90, 0));
                    break;
                case 'E':
                    model_map[i] = Instantiate(wolf_prefab, new Vector3(x, y, z), Quaternion.Euler(0, 180, 0));
                    break;
                case 'S':
                    model_map[i] = Instantiate(wolf_prefab, new Vector3(x, y, z), Quaternion.Euler(0, 90, 0));
                    break;
                case 'Y':
                    model_map[i] = Instantiate(tiger_prefab, new Vector3(x, y, z), Quaternion.Euler(0, 0, 0));
                    break;
                case 'G':
                    model_map[i] = Instantiate(tiger_prefab, new Vector3(x, y, z), Quaternion.Euler(0, -90, 0));
                    break;
                case 'N':
                    model_map[i] = Instantiate(tiger_prefab, new Vector3(x, y, z), Quaternion.Euler(0, 180, 0));
                    break;
                case 'J':
                    model_map[i] = Instantiate(tiger_prefab, new Vector3(x, y, z), Quaternion.Euler(0, 90, 0));
                    break;
                case 'B':
                    model_map[i] = Instantiate(tree_prefab, new Vector3(x, y, z), Quaternion.identity);
                    break;
            }
        }


    }
}
