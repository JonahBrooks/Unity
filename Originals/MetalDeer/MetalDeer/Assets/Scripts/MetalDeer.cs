using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;





public class MetalDeer : MonoBehaviour {

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

        // TODO if I care: public Map(string file_name);

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
            // TODO: Get rid of this method
            Debug.Log(map);
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


    private Queue<Map> generatedMaps;


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


    // TODO: Make this a coroutine that slowly fills a public queue of maps
    // Generates the maps using mu/theta procedural generation
    IEnumerator genMaps()
    {
        int mu = 2;
        int theta = 2;
        int initial_mutations = 15;
        Map[] maps = new Map[mu+theta];
        int tries = 1;
        int num_tries = 100;
        int wins = 0;
        Map.ideal = 30;
        
        // Generate mu+theta random individuals
        for (int i = 0; i < mu + theta; i++)
        {
            maps[i] = new Map();
            // Generate a random starting map
            maps[i].regenerate_map();
            // Advance to add obstacles
            for (int j = 0; j < initial_mutations; j++)
            {
                maps[i].advance_map();
            }
        }

        while (generatedMaps.Count < 100)
        {
            // Evaluate individuals
            for (int i = 0; i < mu + theta; i++)
            {
                tries = 1;
                wins = 0;
                while (tries <= num_tries)
                {
                    if (play(maps[i]) > 0) wins++;
                    tries++;
                    yield return new WaitForFixedUpdate();
                }
                maps[i].rating = 100.0f * (float)(wins) / (float)(num_tries);
                Debug.Log("Done! It was completed " + wins + "/" + num_tries + "(" + maps[i].rating + "%)");
            }
            // Sort by rating
            Array.Sort(maps);

            // Drop theta and evolve all
            for (int i = 0; i < mu + theta; i++)
            {
                yield return new WaitForFixedUpdate();
                Debug.Log(maps[i].rating);
                // Found a suitable map
                if (Mathf.Abs(maps[i].rating - Map.ideal) < 5)
                {
                    generatedMaps.Enqueue(maps[i]);
                    // Generate a random map to replace this one
                    maps[i].regenerate_map();
                    // Advance to add obstacles
                    for (int j = 0; j < initial_mutations; j++)
                    {
                        maps[i].advance_map();
                    }
                }

                // Too hard
                if (maps[i].rating - Map.ideal < 0)
                {
                    maps[i].simplify_map();
                    Debug.Log("Simplifying number " + i + " with rating of " + maps[i].rating);
                }
                else  // Too easy
                {
                    maps[i].advance_map();
                    Debug.Log("Advancing number " + i + " with rating of " + maps[i].rating);
                }

                // Mutate mu of them over theta of them
                if (i >= mu)
                {
                    maps[i] = maps[i - mu];
                    maps[i].simplify_map();
                    maps[i].simplify_map();
                    maps[i].advance_map();
                    maps[i].advance_map();
                }
            }
            Debug.Log("Trying again");
        }
    }


    // Use this for initialization
    void Start () {
        generatedMaps = new Queue<Map>();
        StartCoroutine(genMaps());
	}
	
	// Update is called once per frame
	void Update () {
        //Debug.Log(generatedMaps.Count);
	}
}
