describe('template spec', () => {
  it('passes', () => {
    cy.visit('/');
    cy.injectAxe();
    cy.checkA11y(undefined, { runOnly: ['wcag2a', 'wcag2aa'] });
  });
});
