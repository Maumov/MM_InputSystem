# InputMapper

Permet la création d'inputs plus abstraits.

## Utilisation

La classe InputMapper contient tous les inputs créés. Tout est principalement fait dans l'éditeur de Unity.

Voici les différentes étapes pour créer un input:

1) S'assurer d'avoir un objet InputMapper dans Unity.

2) Ajouter un nouvel objet dans Unity d'un type héritant de ArcadeInput. Attention: Le nom est important, car c'est ce nom qui sera utilisé dans l'enum.

3) Mettre les paramètres voulus dans l'objet.

4) Ajouter l'objet à la liste du InputMapper.

5) Appuyer sur le bouton "Refresh Enum" dans le InputMapper.

Une fois un input créé, il est directement accessible dans le code via InputEnum.(Nom de l'input).Get() .

## Création d'un nouveau type d'input

Il est possible de créer des inputs personnalisés. Pour cela, on crée un script héritant de ArcadeInput. Ensuite, il suffit que ce script appelle d'une certaine manière la fonction Trigger de ArcadeInput afin de déclencher son input.

De base, ce module contient des inputs qui peuvent être utilisés en exemple.

## Exemple d'utilisation

```
public void Start()
{
	InputEnum.DownPress.Get().Triggered += OnDownPressed;
}

public void OnDestroy()
{
	InputEnum.DownPress.Get().Triggered -= OnDownPressed;
}

private void OnDownPressed(ArcadeInput input)
{
	Debug.Log("Down pressed!");
}
