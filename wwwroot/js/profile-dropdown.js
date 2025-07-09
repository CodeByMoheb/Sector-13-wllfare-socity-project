// Profile Dropdown JavaScript
class ProfileDropdown {
    constructor() {
        this.dropdown = document.getElementById('profileDropdown');
        this.trigger = document.querySelector('.user-profile-btn');
        this.isOpen = false;
        this.init();
    }

    init() {
        if (!this.dropdown || !this.trigger) return;
        
        // Add click event to trigger
        this.trigger.addEventListener('click', (e) => {
            e.preventDefault();
            e.stopPropagation();
            this.toggle();
        });

        // Prevent dropdown from closing when clicking inside
        this.dropdown.addEventListener('click', (e) => {
            e.stopPropagation();
        });

        // Prevent dropdown from closing when clicking on action items
        const actionItems = this.dropdown.querySelectorAll('.profile-action-item, form button');
        actionItems.forEach(item => {
            item.addEventListener('click', (e) => {
                e.stopPropagation(); // Do NOT call e.preventDefault() here!
            });
        });

        // Close dropdown when clicking outside
        document.addEventListener('click', (e) => {
            if (!this.dropdown.contains(e.target) && !this.trigger.contains(e.target)) {
                this.close();
            }
        });

        // Close dropdown on escape key
        document.addEventListener('keydown', (e) => {
            if (e.key === 'Escape') {
                this.close();
            }
        });

        // Handle window resize
        window.addEventListener('resize', () => {
            if (this.isOpen) {
                this.close();
            }
        });

        // Add hover effects for action items
        this.addHoverEffects();
    }

    toggle() {
        if (this.isOpen) {
            this.close();
        } else {
            this.open();
        }
    }

    open() {
        if (this.isOpen) return;
        
        this.isOpen = true;
        this.dropdown.style.display = 'block';
        
        // Trigger animation
        requestAnimationFrame(() => {
            this.dropdown.classList.add('show');
        });

        // Add backdrop blur effect
        this.addBackdropBlur();
    }

    close() {
        if (!this.isOpen) return;
        
        this.isOpen = false;
        this.dropdown.classList.remove('show');
        
        // Wait for animation to complete before hiding
        setTimeout(() => {
            if (!this.isOpen) {
                this.dropdown.style.display = 'none';
            }
        }, 300);

        // Remove backdrop blur effect
        this.removeBackdropBlur();
    }

    addBackdropBlur() {
        // Create backdrop element
        const backdrop = document.createElement('div');
        backdrop.className = 'profile-dropdown-backdrop';
        backdrop.style.cssText = `
            position: fixed;
            top: 0;
            left: 0;
            right: 0;
            bottom: 0;
            background: rgba(0, 0, 0, 0.1);
            z-index: 9998;
            opacity: 0;
            transition: opacity 0.3s ease;
        `;
        
        document.body.appendChild(backdrop);
        
        // Animate backdrop
        requestAnimationFrame(() => {
            backdrop.style.opacity = '1';
        });

        // Remove backdrop on click
        backdrop.addEventListener('click', () => {
            this.close();
        });

        // Store reference for removal
        this.backdrop = backdrop;
    }

    removeBackdropBlur() {
        if (this.backdrop) {
            this.backdrop.style.opacity = '0';
            setTimeout(() => {
                if (this.backdrop && this.backdrop.parentNode) {
                    this.backdrop.parentNode.removeChild(this.backdrop);
                }
                this.backdrop = null;
            }, 300);
        }
    }

    addHoverEffects() {
        const actionItems = this.dropdown.querySelectorAll('.profile-action-item');
        
        actionItems.forEach(item => {
            item.addEventListener('mouseenter', () => {
                this.addHoverAnimation(item);
            });
            
            item.addEventListener('mouseleave', () => {
                this.removeHoverAnimation(item);
            });
        });
    }

    addHoverAnimation(element) {
        element.style.transform = 'translateX(4px)';
        element.style.transition = 'transform 0.2s ease';
        
        const icon = element.querySelector('i');
        if (icon) {
            icon.style.transform = 'scale(1.1)';
            icon.style.transition = 'transform 0.2s ease';
        }
    }

    removeHoverAnimation(element) {
        element.style.transform = 'translateX(0)';
        
        const icon = element.querySelector('i');
        if (icon) {
            icon.style.transform = 'scale(1)';
        }
    }
}

// Initialize profile dropdown when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    new ProfileDropdown();
});

// Export for potential use in other scripts
window.ProfileDropdown = ProfileDropdown; 