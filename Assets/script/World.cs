﻿using KBEngine;
using UnityEngine;
using System.Collections;
using System.Linq;
using System; 

public class World : MonoBehaviour {
    //单例模式
    public static World instance;

    private UnityEngine.GameObject terrain = null;
    public UnityEngine.GameObject terrainPerfab;

    private UnityEngine.GameObject player = null;
    public UnityEngine.GameObject entityPerfab;
    public UnityEngine.GameObject avatarPerfab;
    
    static World()
    {
        GameObject go = new GameObject("World");
        DontDestroyOnLoad(go);
        instance = go.AddComponent<World>();
        instance.terrainPerfab = (GameObject)Resources.Load("Terrain");
        instance.entityPerfab = (GameObject)Resources.Load("entity");
        instance.avatarPerfab = (GameObject)Resources.Load("player");
    }
    //激活单例
    public void init()
    {
    }
	// Use this for initialization
	void Start () {
        KBEngine.Event.registerOut("addSpaceGeometryMapping", this, "addSpaceGeometryMapping");
        KBEngine.Event.registerOut("onAvatarEnterWorld", this, "onAvatarEnterWorld");
        KBEngine.Event.registerOut("onEnterWorld", this, "onEnterWorld");
        KBEngine.Event.registerOut("onLeaveWorld", this, "onLeaveWorld");
        KBEngine.Event.registerOut("set_position", this, "set_position");
        KBEngine.Event.registerOut("set_direction", this, "set_direction");
        KBEngine.Event.registerOut("update_position", this, "update_position");
        KBEngine.Event.registerOut("set_name", this, "set_entityName");
        

	}
    void OnDestroy()
    {
        KBEngine.Event.deregisterOut(this);
    }
	// Update is called once per frame
	void Update () {
        createPlayer();
	}

    public void onAvatarEnterWorld(UInt64 rndUUID, Int32 eid, KBEngine.Avatar avatar)
    {
        if (!avatar.isPlayer())
        {
            return;
        }

        //UI.inst.info("loading scene...(加载场景中...)");
        Debug.Log("loading scene...");

        //object speed = avatar.getDefinedPropterty("moveSpeed");
        //if (speed != null)
        //    set_moveSpeed(avatar, speed);

        //object state = avatar.getDefinedPropterty("state");
        //if (state != null)
        //    set_state(avatar, state);

        //object modelScale = avatar.getDefinedPropterty("modelScale");
        //if (modelScale != null)
        //    set_modelScale(avatar, modelScale);

        object name = avatar.getDefinedPropterty("name");
        if (name != null)
            set_entityName(avatar, (string)name);

        //object hp = avatar.getDefinedPropterty("HP");
        //if (hp != null)
        //    set_HP(avatar, hp);
    }

    public void onEnterWorld(KBEngine.Entity entity)
    {
        if (entity.isPlayer())
            return;

        float y = entity.position.y;
        if (entity.isOnGround)
            y = 1.3f;

        entity.renderObj = Instantiate(entityPerfab, new Vector3(entity.position.x, y, entity.position.z),
            Quaternion.Euler(new Vector3(entity.direction.y, entity.direction.z, entity.direction.x))) as UnityEngine.GameObject;

        ((UnityEngine.GameObject)entity.renderObj).name = entity.className + "_" + entity.id;

        set_position(entity);
        set_direction(entity);

        //object speed = entity.getDefinedPropterty("moveSpeed");
        //if (speed != null)
        //    set_moveSpeed(entity, speed);

        //object state = entity.getDefinedPropterty("state");
        //if (state != null)
        //    set_state(entity, state);

        //object modelScale = entity.getDefinedPropterty("modelScale");
        //if (modelScale != null)
        //    set_modelScale(entity, modelScale);

        object name = entity.getDefinedPropterty("name");
        if (name != null)
            set_entityName(entity, (string)name);

        //object hp = entity.getDefinedPropterty("HP");
        //if (hp != null)
        //    set_HP(entity, hp);
    }

    public void onLeaveWorld(KBEngine.Entity entity)
    {
        if (entity.renderObj == null)
            return;

        UnityEngine.GameObject.Destroy((UnityEngine.GameObject)entity.renderObj);
        entity.renderObj = null;
    }
    public void set_entityName(KBEngine.Entity entity, object v)
    {
        if (entity.renderObj != null)
        {
            ((UnityEngine.GameObject)entity.renderObj).GetComponent<GameEntity>().entity_name = (string)v;
        }
    }
    public void set_position(KBEngine.Entity entity)
    {
        if (entity.renderObj == null)
            return;

        ((UnityEngine.GameObject)entity.renderObj).GetComponent<GameEntity>().destPosition = entity.position;
        ((UnityEngine.GameObject)entity.renderObj).GetComponent<GameEntity>().position = entity.position;
    }
    public void set_direction(KBEngine.Entity entity)
    {
        if (entity.renderObj == null)
            return;

        ((UnityEngine.GameObject)entity.renderObj).GetComponent<GameEntity>().destDirection =
            new Vector3(entity.direction.y, entity.direction.z, entity.direction.x);
    }
    public void createPlayer()
    {
        if (player != null)
            return;

        if (KBEngineApp.app.entity_type != "Avatar")
        {
            return;
        }

        KBEngine.Avatar avatar = (KBEngine.Avatar)KBEngineApp.app.player();
        if (avatar == null)
        {
            Debug.Log("wait create(palyer)!");
            return;
        }

        float y = avatar.position.y;
        if (avatar.isOnGround)
            y = 1.3f;

        player = Instantiate(avatarPerfab, new Vector3(avatar.position.x, y, avatar.position.z),
                             Quaternion.Euler(new Vector3(avatar.direction.y, avatar.direction.z, avatar.direction.x))) as UnityEngine.GameObject;

        player.GetComponent<GameEntity>().entityDisable();
        avatar.renderObj = player;
        Camera.main.GetComponent<SmoothFollow>().target = player.transform;
        ((UnityEngine.GameObject)avatar.renderObj).GetComponent<GameEntity>().isPlayer = true;
    }

    public void addSpaceGeometryMapping(string respath)
    {
        Debug.Log("loading scene(" + respath + ")...");
        print("scene(" + respath + "), spaceID=" + KBEngineApp.app.spaceID);
        if (terrain == null)
            terrain = Instantiate(terrainPerfab) as UnityEngine.GameObject;

        player.GetComponent<GameEntity>().entityEnable();
    }

    public void update_position(KBEngine.Entity entity)
    {
        if (entity.renderObj == null)
            return;

        GameEntity gameEntity = ((UnityEngine.GameObject)entity.renderObj).GetComponent<GameEntity>();
        gameEntity.destPosition = entity.position;
        gameEntity.isOnGround = entity.isOnGround;
    }
}