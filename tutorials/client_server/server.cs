// Create a datablock for the observer camera.
datablock CameraData(Observer) {};

datablock PlayerData(BoxPlayer) {
   shapeFile = "tutorials/fps_player/player.dts";
   cameraMaxDist = 5;
   jumpDelay = 0;
};

datablock StaticShapeData(CubeShape) {
   shapeFile = "./cube.dae";
};

function CubeShape::onAdd(%this, %obj) {
   //error(playing SPC %obj.playThread(0, "rise"));
}

// Called by the mainfile when a client has connected to the game and is ready
// to take part!
function GameConnection::onEnterGame(%this) {
   // Create a camera for the client.
   %camera = new Player() {
      datablock = BoxPlayer;
   };
   %camera.setTransform("0 0 2 1 0 0 0");

   // Cameras are not ghosted (sent across the network) by default; we need to
   // do it manually for the client that owns the camera or things will go south
   // quickly.
   //%camera.scopeToClient(%this);
   // And let the client control the camera.
   %this.setControlObject(%camera);

   // Add the camera to the group of objects associated with this connection so
   // it's cleaned up when the client quits.
   %this.add(%camera);
}

// Clean stuff up, notify other clients that the client has left, etc. We don't
// need to delete the camera because we added it as a sub-object of the
// GameConnection, so it will be deleted when this connection is.
function GameConnection::onLeaveGame(%this) {
}

// Called when the engine has been initialised.
function onStart() {
   // Create objects!
   new SimGroup(GameGroup) {
      new LevelInfo(TheLevelInfo) {
         canvasClearColor = "0 0 0";
         visibleDistance = 20;
      };
      new GroundPlane(TheGround) {
         position = "0 0 0";
         material = BlankWhite;
      };
      new Sun(TheSun) {
         azimuth = 230;
         elevation = 45;
         color = "1 1 1";
         ambient = "0.1 0.1 0.1";
         castShadows = true;
      };
      new SimGroup(Cubes) {
         new StaticShape(Cube) {
            datablock = CubeShape;
            position = "0 20 1";
         };
         new StaticShape(Cube2) {
            datablock = CubeShape;
            position = "0 40 1";
         };
         new StaticShape(Cube3) {
            datablock = CubeShape;
            position = "0 60 1";
         };
         new StaticShape(Cube4) {
            datablock = CubeShape;
            position = "0 80 1";
         };
      };
      new AIPlayer() {
         position = "0 20 1";
         datablock = BoxPlayer;
         class = Wanderer;
      };
   };

   Cubes.refresh(1000, 2);
}

function PlayerData::onAdd(%this, %obj) {
   %obj.onAdd();
}

function Wanderer::onAdd(%this) {
   %this.wander();
}

function PlayerData::onReachDestination(%this, %obj) {
   %obj.wander();
}

function Wanderer::wander(%this) {
   %this.setMoveDestination(getRandom(-10, 10) SPC 20 SPC 0);
}

function SimSet::refresh(%this, %period, %objectsPerIteration) {
   %this._refresh = %this.schedule(%period, refresh, %period, %objectsPerIteration);

   if(%this._refreshIndex $= "") {
      %this._refreshIndex = 0;
   }

   %i = %this._refreshIndex;
   %max = %this.getCount()-1;
   for(%j = 0; %j < %objectsPerIteration; %j++) {
      if(%i > %max) {
         %i = 0;
      }
      %obj = %this.getObject(%i);
      %obj.position = %obj.position;
      %i++;
   }

   %this._refreshIndex = %i;
}

// Called when the engine is shutting down.
function onEnd() {
   // Delete the objects we created.
   GameGroup.delete();
}
