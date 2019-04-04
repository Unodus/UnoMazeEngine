using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "New UnoMaze", menuName = "Unomaze")]
public class UnoMaze : ScriptableObject
{
    private int x, y, z;
    private List<Cube> MazeList, luckycubes;
    private Cube[,,] MazeArray;
    private GameObject MazeObject, MyFloorPrefab, MyWallsPrefab;
    private Material MyFloor, MyWalls;
    
    public GameObject GetMazeObject()
    {
        return MazeObject;
    }


    public void Generate(Vector3 pos, Vector3 scale)
    {
        Vector3 startcube = GenerateRandomStart(scale);
        Generate(pos, scale, startcube, null, null, null, null, 0);
    }
    public void Generate(Vector3 pos, Vector3 scale, Vector3 startcube, Material Floor, Material Walls)//override for default mode (0)
    {
        Generate(pos, scale, startcube, Floor, Walls, null, null, 0);       
    }
    public void Generate(Vector3 pos, Vector3 scale, Vector3 startcube, Material Floor, Material Walls, GameObject P_Floor, GameObject P_Walls, int Mode)//override for modes
    {
        x = Mathf.RoundToInt(scale.x);
        y = Mathf.RoundToInt(scale.y);
        z = Mathf.RoundToInt(scale.z);
        MazeArray = new Cube[x, y, z];
        luckycubes = new List<Cube>();
        MazeList = new List<Cube>();
        if (Floor != null) MyFloor = Floor;
        if (Walls != null) MyWalls = Walls;
        if (P_Floor != null) MyFloorPrefab = P_Floor;
        if (P_Walls != null) MyWallsPrefab = P_Walls;

        SearchArray(0);//set outside layer to true

        if (Mode == 0)//Create a Maze around a cube
        {
            Cutout(MazeArray[(int)startcube.x, (int)startcube.y, (int)startcube.z]);// set starting block

            PrimsAlgorithm();
            SearchArray(4);
            Build(pos, scale);
        }
        else if (Mode == 1)//Create a Maze around a sphere
        {
            Cutout(MazeArray[(int)startcube.x, (int)startcube.y, (int)startcube.z]);// set starting block

            PrimsAlgorithm();
            SearchArray(4);
            SphereBuild(pos, scale);
        }
        else if (Mode == 2)//Create a Maze inside a cube
        {
            Cutout(MazeArray[(int)startcube.x, (int)startcube.y, (int)startcube.z]);// set starting block

            PrimsAlgorithm();
            SearchArray(4);
            InsideOutBuild(pos, scale);
        }
        else if(Mode ==3)// Create a Maze inside cube volume
        {

            SearchArray(6);//set all to true
            Cutout(MazeArray[(int)startcube.x, (int)startcube.y, (int)startcube.z]);// set starting block

            PrimsAlgorithm();
            SearchArray(4);
            NoFloorBuild(pos, scale);
            //custom builder?
        }
        else if (Mode == 4)//[Unfinished] Create a flat maze
        {
            Cutout(MazeArray[(int)startcube.x, (int)startcube.y, (int)startcube.z]);// set starting block

            PrimsAlgorithm();
            SearchArray(4);
            BottomFloorBuild(pos, scale);
        }
        else if (Mode == 5)//[Unfinished] Create a Maze around Cylinder
        {
            //set all cubes to true
            SearchArray(7);//set y's to false
            Cutout(MazeArray[(int)startcube.x, (int)startcube.y, (int)startcube.z]);// set starting block
            
            PrimsAlgorithm();
            SearchArray(4);
            CylinderBuild(pos, scale);
        }
        //possible expansions: 2d maze, maze inside a sphere, maze around a cylinder 
    }


    public void ReGenerate(Vector3 pos, Vector3 scale)
    {
 //       Vector3 startcube = GenerateRandomStart(scale);
   //     Generate(pos, scale, startcube, null, null, null, null, 0);
    }

    public void Delete()
    {
        Destroy(MazeObject);
    }

    private GameObject InitBuild(Vector3 pos, Vector3 scale)
    {
        MazeObject = new GameObject();
        MazeObject.transform.position = pos;
        MazeObject.transform.localScale = scale;
        return MazeObject;
    }

    public void Build(Vector3 pos, Vector3 scale)
    {
        MazeObject = InitBuild(pos, scale);

        GameObject MazeBase;
        if (MyFloorPrefab) MazeBase = Instantiate(MyFloorPrefab); 
        else MazeBase = GameObject.CreatePrimitive(PrimitiveType.Cube); 
        MazeBase.transform.parent = MazeObject.transform;
        MazeBase.transform.localPosition = new Vector3(0,0,0);
        MazeBase.transform.localScale = new Vector3(((scale.x - 1) / scale.x), ((scale.y - 1) / scale.y), ((scale.z - 1) / scale.z));
        if(MyFloor) MazeBase.GetComponent<Renderer>().material = MyFloor;

        SearchArray(2);

    }
    public void CylinderBuild(Vector3 pos, Vector3 scale)
    {
        MazeObject = InitBuild(pos, scale);

        GameObject MazeBase;
        if (MyFloorPrefab) MazeBase = Instantiate(MyFloorPrefab);
        else MazeBase = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        MazeBase.transform.parent = MazeObject.transform;
        MazeBase.transform.localPosition = new Vector3(0, 0, 0);
        MazeBase.transform.localScale = new Vector3(((scale.x - 1) / scale.x), ((scale.y - 1) / scale.y)*0.5f, ((scale.z - 1) / scale.z));
        if (MyFloor) MazeBase.GetComponent<Renderer>().material = MyFloor;

        GameObject[] MazeBases = new GameObject[2];
        for (int i = 0; i < 2; i++)
        {
            MazeBases[i] = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            MazeBases[i].transform.parent = MazeObject.transform;
            if (MyFloor)
            {
                MazeBases[i].GetComponent<Renderer>().material = MyFloor;
            }
            MazeBases[i].name = "Floortile";
            MazeBases[i].transform.localScale = new Vector3(1f, (1/scale.y), 1f);
        }
        float Locposition = 0.5f;
        MazeBases[0].transform.localPosition = new Vector3(0, Locposition, 0);
    
        MazeBases[1].transform.localPosition = new Vector3(0, -Locposition, 0);
     
        SearchArray(8);

    }
    public void BottomFloorBuild(Vector3 pos, Vector3 scale)
    {
        MazeObject = InitBuild(pos, scale);

        SearchArray(2);
        GameObject MazeBase;
        if (MyFloorPrefab) MazeBase = Instantiate(MyFloorPrefab);
        else MazeBase = GameObject.CreatePrimitive(PrimitiveType.Cube);
        MazeBase.transform.parent = MazeObject.transform;
        MazeBase.transform.localScale = new Vector3(((scale.x ) / scale.x), ((scale.y ) / scale.y), ((scale.z ) / scale.z));
        if (MyFloor) MazeBase.GetComponent<Renderer>().material = MyFloor;


        if      (y == 1) MazeBase.transform.localPosition = new Vector3(0, -1, 0); 
        else if (x == 1) MazeBase.transform.localPosition = new Vector3(-1, 0, 0); 
        else if (z == 1) MazeBase.transform.localPosition = new Vector3(0, 0, -1); 

    }

    public void NoFloorBuild(Vector3 pos, Vector3 scale)
    {
        MazeObject = InitBuild(pos, scale);
        SearchArray(2);
    }

    public void SphereBuild(Vector3 pos, Vector3 scale)
    {
        MazeObject = InitBuild(pos, scale);

        SearchArray(5);
        GameObject MazeBase;
        if (MyFloorPrefab) MazeBase = Instantiate(MyFloorPrefab);
        else MazeBase = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        MazeBase.transform.parent = MazeObject.transform;
        MazeBase.transform.localPosition = new Vector3(0, 0, 0);
        MazeBase.transform.localScale = new Vector3(((scale.x - 1) / scale.x), ((scale.y - 1) / scale.y), ((scale.z - 1) / scale.z));
        if (MyFloor) MazeBase.GetComponent<Renderer>().material = MyFloor;
    }

    public void InsideOutBuild(Vector3 pos, Vector3 scale)
    {
        MazeObject = InitBuild(pos, scale);

        SearchArray(2);
        float Locposition = 0.5f;
        GameObject[] MazeBases = new GameObject[6];
        for(int i = 0; i< 6; i++)
        {
            MazeBases[i] = GameObject.CreatePrimitive(PrimitiveType.Plane);
            MazeBases[i].transform.parent = MazeObject.transform;
            if (MyFloor)
            {
                MazeBases[i].GetComponent<Renderer>().material = MyFloor;
            }
            MazeBases[i].name = "Floortile";
            MazeBases[i].transform.localScale = new Vector3(0.1f, 1, 0.1f);
            }

        MazeBases[0].transform.localPosition = new Vector3(Locposition, 0, 0);
        MazeBases[0].transform.eulerAngles =   new Vector3(0, 0, 90);

        MazeBases[1].transform.localPosition = new Vector3(0, Locposition, 0);
        MazeBases[1].transform.eulerAngles = new Vector3(180, 0, 0);

        MazeBases[2].transform.localPosition = new Vector3(0, 0, Locposition);
        MazeBases[2].transform.eulerAngles = new Vector3(-90, 0, 0);

        MazeBases[3].transform.localPosition = new Vector3(-Locposition, 0, 0);
        MazeBases[3].transform.eulerAngles = new Vector3(0, 0, -90);

        MazeBases[4].transform.localPosition = new Vector3(0, -Locposition, 0);
  
        MazeBases[5].transform.localPosition = new Vector3(0, 0, -Locposition);
        MazeBases[5].transform.eulerAngles = new Vector3(90, 0, 0);
       
    }
    public Vector3 GenerateRandomStart(Vector3 scale)
    {
        x = Mathf.RoundToInt(scale.x);
        y = Mathf.RoundToInt(scale.y);
        z = Mathf.RoundToInt(scale.z);
        MazeArray = new Cube[x, y, z];
        luckycubes = new List<Cube>();
        MazeList = new List<Cube>();
        SearchArray(0);//set outside layer to true
        Cube newcube = SearchArray(1);
        return new Vector3(newcube.GetX(), newcube.GetY(), newcube.GetZ());
    }


    private Cube SearchArray(int mode)//searches through array via for loop, mode decides what to do with what is found
    {// mode 0 initiate carveability
        // mode 1 select random cube
        // mode 2 generate cubes
        // mode 3 delete all cubes on maze
        // mode 4 assign color and models to all cubes
        // mode 5 perform extra calculations for sphere-mode
        int count = 0;
        for (int xi = 0; xi < x; xi++)
        {
            for (int yi = 0; yi < y; yi++)
            {
                for (int zi = 0; zi < z; zi++)
                {
                    switch (mode)
                    {
                        case 0:
                            if (zi == 0 || yi == 0 || xi == 0 || zi == z - 1 || yi == y - 1 || xi == x - 1)
                                {

                                MazeArray[xi, yi, zi].SetCarveable(true);
                                MazeArray[xi, yi, zi].SetDeletable(false);
                                MazeArray[xi, yi, zi].SetPos(xi, yi, zi);
                                MazeArray[xi, yi, zi].SetWeight(Random.Range(1, 199));

                            }
                            else
                                {
                                    MazeArray[xi, yi, zi].SetCarveable(false);
                                    MazeArray[xi, yi, zi].SetDeletable(true);
                            }
                            
                            break;
                        case 1:
                            if (MazeArray[xi, yi, zi].GetCarveable() ==true)
                            {
                        
                                luckycubes.Add(MazeArray[xi, yi, zi]);
                                count++;
                            }
                            break;
                        case 2:
                            if (MazeArray[xi, yi, zi].GetCarveable()== true)
                            {
                                MazeArray[xi, yi, zi].Generate(MazeObject);
                            }
                            break;
                        case 3:
                            MazeArray[xi, yi, zi].Delete();
                            MazeArray[xi, yi, zi]= new Cube();
                            break;
                        case 4:
                            if (MazeArray[xi, yi, zi].GetCarveable() == true)
                            {
                                if(MyWalls) MazeArray[xi, yi, zi].SetColor(MyWalls);
                                if (MyWallsPrefab) MazeArray[xi, yi, zi].SetPrefab(MyWallsPrefab);
                            }
                            break;
                        case 5:
                            if (MazeArray[xi, yi, zi].GetCarveable() == true)
                            {

                                MazeArray[xi, yi, zi].SphereGenerate(MazeObject);
                            }
                            break;
                        case 6:
                                MazeArray[xi, yi, zi].SetCarveable(true);
                                MazeArray[xi, yi, zi].SetDeletable(false);
                                MazeArray[xi, yi, zi].SetPos(xi, yi, zi);
                                MazeArray[xi, yi, zi].SetWeight(Random.Range(1, 199));

                            break;
                        case 7:
                            if (yi == 0 || yi == y - 1)
                            {
                                MazeArray[xi, yi, zi].SetCarveable(false);
                                MazeArray[xi, yi, zi].SetDeletable(true);
                            }
                            else if (MazeArray[xi,yi,zi].GetCarveable())
                            {
                                MazeArray[xi, yi, zi].SetCarveable(true);
                                MazeArray[xi, yi, zi].SetDeletable(false);
                            }
                                break;
                        case 8:
                            if (MazeArray[xi, yi, zi].GetCarveable() == true)
                            {
                                MazeArray[xi, yi, zi].CylGenerate(MazeObject);
                            }
                            break;
                        default:
                            Debug.Log("Mode Input Error"); 
                            break;
                    }
                    
                    
                }
            }
        }

        if (mode == 1)
        {
            int rand = Random.Range(0, count);
            return luckycubes[rand];
        }

        Cube badsearch = new Cube();
        badsearch.SetCarveable(false);
        return badsearch;
    }


    private void Cutout(Cube tCube)//call this function to cut a cube out of the maze to create a path
    {
        if (tCube.GetCarveable() == true)
        {
            MazeList.Add(MazeArray[tCube.GetX(), tCube.GetY(), tCube.GetZ()]);
            MazeArray[tCube.GetX(), tCube.GetY(), tCube.GetZ()].SetCarveable(false);
        }
    }

    private void PrimsAlgorithm()
    {
        bool isayso = true;
        Cube temp = new Cube();
        Cube TargetCube = new Cube();
        while (isayso)
        {

            TargetCube.SetWeight(999);
            foreach(Cube i in MazeList)
            {
                luckycubes.Clear();
                temp = NeighbourCubes(i.GetX(), i.GetY(), i.GetZ(), 0);
                if( MazeArray[temp.GetX() , temp.GetY() , temp.GetZ()].GetWeight() < TargetCube.GetWeight() && temp.GetCarveable()== true)
                {
                    TargetCube = temp;
                }
            }
            if (TargetCube.GetWeight() == 999) isayso = false;
            else Cutout(TargetCube);
        }
    }
    private Cube NeighbourCubes(int pointX, int pointY, int pointZ, int mode)//for loop for points 1 to -1 for x y and z of cube. Mode for function
    {
        
        //mode 0: initial check for adjacent neighbour squares
        //mode 1: checks that only one active neighbour is adjacent
        for (int xi = -1; xi <= 1; xi++)
        {
            for (int yi = -1; yi <= 1; yi++)
            {
                for (int zi = -1; zi <= 1; zi++)
                {
                    switch (mode)
                    {
                        case 0:

                            if (pointX + xi >= 0 && pointX + xi < x && pointY + yi >= 0 && pointY + yi < y && pointZ + zi >= 0 && pointZ + zi < z && !(pointX == 0 && pointY == 0 && pointZ == 0))
                            {//return true if value is within array and not a diagonal or itself, else false
                                if (MazeArray[pointX + xi, pointY + yi, pointZ + zi].GetCarveable())
                                {
                                    luckycubes.Clear();
                                    NeighbourCubes(pointX + xi, pointY + yi, pointZ + zi, 1);
                                    if (luckycubes.Count == 1)
                                    {
                                    luckycubes.Clear();
                                    return MazeArray[pointX + xi, pointY + yi, pointZ + zi];
                                    }
                                }
                            }
                        break;
                        case 1:
                            if (!(Mathf.Pow(xi, 2) == 1 && Mathf.Pow(yi, 2) == 1 || Mathf.Pow(xi, 2) == 1 && Mathf.Pow(zi, 2) == 1 || Mathf.Pow(zi, 2) == 1 && Mathf.Pow(yi, 2) == 1) && !(xi == 0 && yi == 0 && zi == 0))
                            {//return true if value is within array and not a diagonal or itself, else false
                                if (pointX + xi >= 0 && pointX + xi < x && pointY + yi >= 0 && pointY + yi < y && pointZ + zi >= 0 && pointZ + zi < z && !(pointX == 0 && pointY == 0 && pointZ == 0))
                                {
                                    if (MazeArray[pointX + xi, pointY + yi, pointZ + zi].GetCarveable()==false && MazeArray[pointX + xi, pointY + yi, pointZ + zi].GetDeletable() == false)
                                    {
                                        luckycubes.Add(MazeArray[pointX, pointY , pointZ ]);
                                    }
                                }
                            }
                            break;
                        default:
                            Debug.Log("Mode Input Error");
                            break;
                    }

                }

            }
        }
        Cube badsearch = new Cube();
        badsearch.SetWeight(999);
        return badsearch;
    }

    private struct Cube
    {
        private int x, y, z;
        private float weight;   
        private bool carveable, toDelete;
        private GameObject cube, prefab;
        private Material MyColor;


        private GameObject InitBuild()
        {
            GameObject makecube;
            if (prefab == null)
            {
                makecube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            }
            else
            {
                makecube = Instantiate(prefab, new Vector3((x / 2) - x, (y / 2) - y, (z / 2) - z), Quaternion.identity);
            }
            makecube.name = x + " " + y + " " + z;
            if (carveable)
            {
                if (MyColor)
                {
                    makecube.GetComponent<Renderer>().material = MyColor;

                }
                else
                {
                    makecube.GetComponent<Renderer>().material.color = Color.red;
                }
            }
            return makecube;
        }

        public void CylGenerate(GameObject Parent)
        {
            cube = InitBuild();
            cube.transform.parent = Parent.transform;
            Vector3 NewPostion = new Vector3((x + 0.5f - (Parent.transform.localScale.x / 2)) / Parent.transform.localScale.x, (y + 0.5f - (Parent.transform.localScale.y / 2)) / Parent.transform.localScale.y, (z + 0.5f - (Parent.transform.localScale.z / 2)) / Parent.transform.localScale.z);
            NewPostion = Vector3.Normalize(NewPostion) / 2;
            cube.transform.localPosition = new Vector3(NewPostion.x, (y + 0.5f - (Parent.transform.localScale.y / 2)) / Parent.transform.localScale.y, NewPostion.z);
            Vector3 newRotation = (new Vector3(Parent.transform.localPosition.x, cube.transform.localPosition.y, Parent.transform.localPosition.z))- cube.transform.localPosition;
            Quaternion rotation = Quaternion.LookRotation(newRotation, Vector3.up);
            cube.transform.localRotation= rotation;
        }
        public void SphereGenerate(GameObject Parent)
        {
            cube = InitBuild(); 
       //     cube.transform.localScale = new Vector3(Mathf.Cos((y - Mathf.FloorToInt(Parent.transform.localScale.y / 2)) * Parent.transform.localScale.y / 2), 1, 1);
            cube.transform.parent = Parent.transform;
            Vector3 NewPostion = new Vector3((x + 0.5f - (Parent.transform.localScale.x / 2)) / Parent.transform.localScale.x, (y + 0.5f - (Parent.transform.localScale.y / 2)) / Parent.transform.localScale.y, (z + 0.5f - (Parent.transform.localScale.z / 2)) / Parent.transform.localScale.z);
            NewPostion = Vector3.Normalize(NewPostion) / 2;   
            cube.transform.localPosition = NewPostion;

            Vector3 newRotation = (Parent.transform.position- cube.transform.localPosition);
            Quaternion rotation = Quaternion.LookRotation(newRotation, Vector3.up);
            cube.transform.localRotation = rotation;

        }

        public void Generate(GameObject Parent) //instantiate cube
        {
            cube = InitBuild();
            cube.transform.parent = Parent.transform;
            cube.transform.localPosition = new Vector3((x + 0.5f - (Parent.transform.localScale.x / 2)) / Parent.transform.localScale.x, (y + 0.5f - (Parent.transform.localScale.y / 2)) / Parent.transform.localScale.y, (z + 0.5f - (Parent.transform.localScale.z / 2)) / Parent.transform.localScale.z);
        }
        public void Delete() {Destroy(cube);}
        public float GetWeight() { return weight; }
        public void SetWeight(float input) { weight = input; }
        public bool GetCarveable() { return carveable; }
        public void SetCarveable(bool NewCarveable) { carveable = NewCarveable; }
        public bool GetDeletable() { return toDelete; }
        public void SetDeletable(bool NewBool) { toDelete = NewBool; }
        public void SetColor(Material matt) { MyColor = matt; }
        public void SetPrefab(GameObject fab) { prefab = fab; }


        public int GetX() { return x; }
        public int GetY() { return y; }
        public int GetZ() { return z; }
        public void SetPos(int xpos, int ypos, int zpos)
        {
            x = xpos;
            y = ypos;
            z = zpos;
        }
    }
}
