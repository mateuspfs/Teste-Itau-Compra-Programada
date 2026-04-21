import Button from './Button';

// Grupo de botões de ação padronizado (Editar/Excluir)
interface ActionButtonsProps {
  onEdit?: () => void;
  onDelete?: () => void;
  editLabel?: string;
  deleteLabel?: string;
}

export default function ActionButtons({
  onEdit,
  onDelete,
  editLabel = 'Editar',
  deleteLabel = 'Excluir',
}: ActionButtonsProps) {
  return (
    <div className="flex items-center justify-end gap-2">
      {onEdit && (
        <Button variant="secondary" size="sm" onClick={onEdit}>
          {editLabel}
        </Button>
      )}
      {onDelete && (
        <Button variant="danger" size="sm" onClick={onDelete}>
          {deleteLabel}
        </Button>
      )}
    </div>
  );
}

